using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http
{
    public class ResponseBuilder : MessageBuilder
    {
        public ResponseBuilder(Http.Request request)
        {
            Request = request;
            AllHeadersReceived = false;
            MessageSize = -1;
        }

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
                    newpacket.StartsWith("HTTP") &&
                    newpacket.Contains("\r\n"))
                {
                    string temp = newpacket.Substring(0, newpacket.IndexOf("\r\n"));
                    Response.StatusLine = StatusLine.Parse(temp);
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
                _remainingBuffer = TrimStartBuffer(_remainingBuffer, index+4); // +4 because we want to remove the trailing \r\n\r\n also
                // Reduce the append position by the number of removed bytes
                _remainingBufferAppendPosition -= index+4; // +4 for same reason

                lines = GetLines(_firstLineAndHeaders);

                // We cycle thru the lines to get the most recent status line
                while (lines[loop].StartsWith("HTTP"))
                {
                    loop++;
                }

                if (Response != null &&
                    Response.StatusLine != null &&
                    !string.IsNullOrEmpty(Response.StatusLine.HttpVersion))
                {
                    // If we are here, we have received a 100-continue status earlier
                    if (Response.StatusLine.StatusCode != 100)
                        throw new HttpNetworkStreamException("Unexpected status code.");
                }
                else
                {
                    // If we are here, we have no status line yet, need to parse one

                    if (loop <= 0)
                        throw new HttpNetworkStreamException("Status line not found.");

                    // Now loop holds the index of the NEXT line after the last HTTP line, so decrement 1
                    // to get back to the index
                    loop--;

                    Response = new Response(Request);
                    Response.StatusLine = StatusLine.Parse(lines[loop]);
                }

                // No matter if we received the 100 or not, we can now parse headers

                Response.Headers.Clear();

                // We go loop+1 because we want to skip the status line
                for (int i = loop+1; i < lines.Count; i++)
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
            HttpNetworkStream ns;
            byte[] newBuffer = null;

            AsyncCallback callback = (AsyncCallback)e.UserToken;

            AppendAndParse(e.Buffer, 0, e.BytesTransferred);
            if (AllHeadersReceived)
            {
                // Sets up the buffer to prepend
                if (_remainingBufferAppendPosition > 0)
                {
                    // We need to take the left over buffer from _responseBuilder and prepend that
                    // to an HttpNetworkStream wrapping the _tcpConnection and then give the user
                    // that HttpNetworkStream... cake

                    newBuffer = new byte[_remainingBufferAppendPosition];
                    BytesReceived += e.BytesTransferred - newBuffer.Length;
                    Buffer.BlockCopy(_remainingBuffer, 0, newBuffer, 0, newBuffer.Length);
                }
                else
                {
                    newBuffer = null;
                }


                
                if (!Response.ContentLength.HasValue)
                { // If content length is not set
                    if (!Response.Headers.ContainsKey(new Message.ChunkedTransferEncodingHeader()))
                    {
                        throw new HttpNetworkStreamException("A Content-Length header was not found.");
                    }

                    MessageSize = 0;

                    // NetworkStream needs modified to handle undetermined length
                    ns = new HttpNetworkStream(HttpNetworkStream.DirectionType.Download, newBuffer, 
                        sender.Socket, System.IO.FileAccess.Read, false);

                    Response.Body.IsChunked = true;
                    Response.Body.ReceiveStream = new Interceptors.InterceptorStream(new Interceptors.ChunkedEncodingInterceptor(ns));        
                }
                else
                { // Content length is set
                    if (Response.Headers.ContainsKey(new Message.ChunkedTransferEncodingHeader()))
                    {
                        throw new HttpNetworkStreamException("A Content-Length header was found in a chunked transfer.");
                    }

                    ulong temp = (ulong)Response.ContentLength.Value;
                    MessageSize = BytesReceived + Response.ContentLength.Value;

                    ns = new HttpNetworkStream(HttpNetworkStream.DirectionType.Download,
                        temp, newBuffer, sender.Socket, System.IO.FileAccess.Read, false);
                    Response.Body.ReceiveStream = ns;
                }


                callback(this, Response);
            }
            else
            {
                BytesReceived += e.BytesTransferred;
                sender.ReceiveAsync(ParseAndAttachToBody_Callback, callback);
            }
        }
    }
}
