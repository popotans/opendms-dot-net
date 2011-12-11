using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http
{
    public abstract class MessageBuilder
    {
        public delegate void AsyncCallback(MessageBuilder sender, Message.Base message);

        protected string _firstLineAndHeaders;
        protected byte[] _remainingBuffer;
        protected int _remainingBufferAppendPosition;
        
        public Request Request { get; set; }
        public Response Response { get; set; }
        public string RawFirstLineAndHeaders { get { return _firstLineAndHeaders; } }
        public byte[] RawRemainingBuffer { get { return _remainingBuffer; } }
        public bool AllHeadersReceived { get; protected set; }
        public long BytesReceived { get; protected set; }
        public long MessageSize { get; protected set; }


        public MessageBuilder()
        {
            AllHeadersReceived = false;
            MessageSize = -1;
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


        public void ParseAndAttachToBody(Tcp.TcpConnection connection, AsyncCallback callback)
        {
            // We are not worrying about onProgress, onTimeout, onError because the developer should 
            // already have those listeners attached

            connection.ReceiveAsync(ParseAndAttachToBody_Callback, callback);
        }

        public abstract void Parse();
        protected abstract void ParseAndAttachToBody_Callback(Tcp.TcpConnection sender, Tcp.TcpConnectionAsyncEventArgs e);

        protected List<string> GetLines(string statusAndHeaders)
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

        protected byte[] TrimStartBuffer(byte[] buffer, int offset)
        {
            if (offset < 0)
                throw new IndexOutOfRangeException();

            byte[] newBuffer = new byte[buffer.Length - offset];
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, newBuffer.Length);

            return newBuffer;
        }

        protected byte[] ResizeBuffer(byte[] buffer, int length)
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

        protected string BytesToString(byte[] buffer, int offset, int length)
        {
            return System.Text.Encoding.ASCII.GetString(buffer, offset, length);
        }

        protected int IndexOf(byte[] source, byte[] search)
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
