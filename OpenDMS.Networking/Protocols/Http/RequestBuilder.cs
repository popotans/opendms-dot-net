using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http
{
    public class RequestBuilder : MessageBuilder
    {
        public override void Parse()
        {
            // A status line will always be first, its possible another status line will follow 
            // (consider 100 continue then a 404)

            int index;
            string newpacket;


            newpacket = BytesToString(_remainingBuffer, 0, _remainingBufferAppendPosition);

            if ((index = newpacket.IndexOf("\r\n\r\n")) < 0)
            {
                // Ending sequence is not found, we need to append to our string and wait for
                // Parse to get called again.
                _firstLineAndHeaders += newpacket;

                // Clear _remainingBuffer since it was all used
                _remainingBuffer = null;
                _remainingBufferAppendPosition = 0;


                if (!string.IsNullOrEmpty(_firstLineAndHeaders) &&
                    (newpacket.StartsWith("GET") || newpacket.StartsWith("DELETE") ||
                    newpacket.StartsWith("HEAD") || newpacket.StartsWith("POST") ||
                    newpacket.StartsWith("PUT")))
                {
                    string temp = newpacket.Substring(0, newpacket.IndexOf("\r\n")).Trim();
                    RequestLine rl = RequestLine.Parse(temp);
                    Request = new Request(rl.Method, rl.RequestUri);
                }
            }
            else
            {
                string[] parts;
                List<string> lines;
                int loop = 0;

                // index + 4 = how many bytes to skip in _remainingBuffer then tie the stream to the
                // Response.Body

                // Possible index placements (index < 0 is handled and index >= newpacket.length-4 is impossible
                // 1) index == 0
                // 2) index between 0 and newpacket.length-4

                // Append the headers from newpacket to the other status and headers
                _firstLineAndHeaders += newpacket.Substring(0, index);
                // Reduce the buffer by the removed bytes
                _remainingBuffer = TrimStartBuffer(_remainingBuffer, index);
                // Reduce the append position by the number of removed bytes
                _remainingBufferAppendPosition -= index;

                lines = GetLines(_firstLineAndHeaders);

                if (Request == null || Request.RequestLine == null)
                {
                    // We have no status line yet, need to parse one
                    string temp = newpacket.Substring(0, newpacket.IndexOf("\r\n")).Trim();
                    RequestLine rl = RequestLine.Parse(temp);
                    Request = new Request(rl.Method, rl.RequestUri);
                }
                
                Request.Headers.Clear();
                for (int i = loop; i < lines.Count; i++)
                {
                    parts = new string[2];
                    parts[0] = lines[i].Substring(0, lines[i].IndexOf(':')).Trim();
                    parts[1] = lines[i].Substring(lines[i].IndexOf(':') + 1).Trim();

                    Response.Headers.Add(new Message.Token(parts[0]), parts[1]);
                }

                AllHeadersReceived = true;
            }
        }

        protected override void ParseAndAttachToBody_Callback(Tcp.TcpConnection sender, Tcp.TcpConnectionAsyncEventArgs e)
        {
            AsyncCallback callback = (AsyncCallback)e.UserToken;

            AppendAndParse(e.Buffer, 0, e.Length);
            if (AllHeadersReceived)
            {
                if (!Request.ContentLength.HasValue)
                    throw new HttpNetworkStreamException("A Content-Length header was not found.");

                ulong temp = (ulong)Request.ContentLength.Value;

                // We need to take the left over buffer from _responseBuilder and prepend that
                // to an HttpNetworkStream wrapping the _tcpConnection and then give the user
                // that HttpNetworkStream... cake

                byte[] newBuffer = new byte[_remainingBufferAppendPosition];

                BytesReceived += e.Length - newBuffer.Length;
                MessageSize = BytesReceived + Request.ContentLength.Value;
                Buffer.BlockCopy(_remainingBuffer, 0, newBuffer, 0, newBuffer.Length);

                HttpNetworkStream ns = new HttpNetworkStream(HttpNetworkStream.DirectionType.Upload,
                    temp, newBuffer, sender.Socket, System.IO.FileAccess.Write, false);

                if (Request.Headers.ContainsKey(new Message.ChunkedTransferEncodingHeader()))
                    Request.Body.IsChunked = true;

                Response.Body.ReceiveStream = ns;

                callback(this, Response);
            }
            else
            {
                BytesReceived += e.Length;
                sender.ReceiveAsync(ParseAndAttachToBody_Callback, callback);
            }
        }

    }
}
