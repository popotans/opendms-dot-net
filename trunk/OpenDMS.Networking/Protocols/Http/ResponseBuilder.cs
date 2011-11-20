using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http
{
    public class ResponseBuilder
    {
        public delegate void AsyncCallback(ResponseBuilder sender, Response response);

        private string _statusAndHeaders;
        private byte[] _remainingBuffer;
        private int _remainingBufferAppendPosition;

        public Response Response { get; set; }
        public string RawStatusAndHeaders { get { return _statusAndHeaders; } }
        public byte[] RawRemainingBuffer { get { return _remainingBuffer; } }
        public bool AllHeadersReceived { get; private set; }
        

        public ResponseBuilder()
        {
            AllHeadersReceived = false;
        }

        public void AppendAndParse(byte[] buffer, int offset, int length)
        {
            Append(buffer, offset, length);
            Parse();
        }

        public void Append(byte[] buffer, int offset, int length)
        {
            if (_remainingBuffer != null && _remainingBuffer.Length > 0)
            {
                ResizeBuffer(_remainingBuffer, length + _remainingBuffer.Length);
            }
            else
            {
                ResizeBuffer(_remainingBuffer, length);
            }
            Buffer.BlockCopy(buffer, offset, _remainingBuffer, _remainingBufferAppendPosition, length);
            _remainingBufferAppendPosition += length;
        }

        public void Parse()
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
                _statusAndHeaders += newpacket;

                // Clear _remainingBuffer since it was all used
                _remainingBuffer = null;
                _remainingBufferAppendPosition = 0;


                if (!string.IsNullOrEmpty(_statusAndHeaders) &&
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
                _statusAndHeaders += newpacket.Substring(0, index);
                // Reduce the buffer by the removed bytes
                _remainingBuffer = TrimStartBuffer(_remainingBuffer, index);
                // Reduce the append position by the number of removed bytes
                _remainingBufferAppendPosition -= index;

                lines = GetLines(_statusAndHeaders);

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

                    Response = new Response();
                    Response.StatusLine = StatusLine.Parse(lines[loop]);
                }

                // No matter if we received the 100 or not, we can now parse headers

                Response.Headers.Clear();
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

        public void ParseAndAttachToResponseBody(Tcp.TcpConnection connection, AsyncCallback callback)
        {
            // We are not worrying about onProgress, onTimeout, onError because the developer should 
            // already have those listeners attached

            connection.ReceiveAsync(ParseAndAttachToResponseBody_Callback, callback);
        }

        private void ParseAndAttachToResponseBody_Callback(Tcp.TcpConnection sender, Tcp.TcpConnectionAsyncEventArgs e)
        {
            AsyncCallback callback = (AsyncCallback)e.UserToken;

            AppendAndParse(e.Buffer, 0, e.Length);
            if (AllHeadersReceived)
            {
                if (!Response.ContentLength.HasValue)
                    throw new HttpNetworkStreamException("A Content-Length header was not found.");

                ulong temp = (ulong)Response.ContentLength.Value;

                // We need to take the left over buffer from _responseBuilder and prepend that
                // to an HttpNetworkStream wrapping the _tcpConnection and then give the user
                // that HttpNetworkStream... cake

                byte[] newBuffer = new byte[_remainingBufferAppendPosition];
                Buffer.BlockCopy(_remainingBuffer, 0, newBuffer, 0, newBuffer.Length);

                HttpNetworkStream ns = new HttpNetworkStream(HttpNetworkStream.DirectionType.Download,
                    temp, newBuffer, sender.Socket, System.IO.FileAccess.Read, false);

                if (Response.Headers.ContainsKey(new Message.ChunkedTransferEncodingHeader()))
                    Response.Body.IsChunked = true;

                Response.Body.Stream = ns;

                callback(this, Response);
            }
            else
            {
                sender.ReceiveAsync(ParseAndAttachToResponseBody_Callback, callback);
            }
        }

        private List<string> GetLines(string statusAndHeaders)
        {
            List<string> ret = new List<string>();
            string[] lines;

            statusAndHeaders = statusAndHeaders.TrimStart('\r', '\n');

            lines = statusAndHeaders.Split('\r', '\n');

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
                if (lines[i].Length > 5)
                    ret.Add(lines[i]);
            }

            return ret;
        }

        private byte[] TrimStartBuffer(byte[] buffer, int offset)
        {
            if (offset < 0)
                throw new IndexOutOfRangeException();

            byte[] newBuffer = new byte[buffer.Length - offset];
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, newBuffer.Length);

            return newBuffer;
        }

        private byte[] ResizeBuffer(byte[] buffer, int length)
        {
            byte[] newBuffer = new byte[length];

            if (buffer == null)
                buffer = new byte[length];
            else
            {
                if (buffer.Length < length)
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
                else
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, length);
            }

            return newBuffer;
        }

        private string BytesToString(byte[] buffer, int offset, int length)
        {
            return System.Text.Encoding.ASCII.GetString(buffer, offset, length);
        }

        private int IndexOf(byte[] source, byte[] search)
        {
            bool fail = true;

            for (int i = 0; i < source.Length; i++)
            {
                // If the buffer index has the same value as the current index of "bytes" then match
                if (source[i] == search[0])
                {
                    // Scan down the _buffer to see if we have a match
                    for (int j = 1; j < search.Length; j++)
                    {
                        // If the buffer index at i (initial loc) + j (the current index we are looking at in bytes)
                        // do not match, then we know we do not have a match
                        if (source[i + j] != search[j])
                        {
                            fail = true;
                            break;
                        }
                    }

                    if (!fail)
                        return i;
                }
            }

            return -1;
        }
    }
}
