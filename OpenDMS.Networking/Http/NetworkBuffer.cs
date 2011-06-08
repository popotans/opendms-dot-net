using System;

namespace OpenDMS.Networking.Http
{
    public class NetworkBuffer
    {
        private byte[] _buffer = null;

        public int Length { get { return _buffer.Length; } }
        public byte[] Buffer { get { return _buffer; } }

        public NetworkBuffer()
        {
        }

        public NetworkBuffer(byte[] bytes)
        {
            _buffer = bytes;
        }

        public byte this[int index]
        {
            get { return _buffer[index]; }
            set { _buffer[index] = value; }
        }

        public NetworkBuffer GetSubBuffer(int offset, int length, bool removeFromOriginal)
        {
            byte[] newBuffer = new byte[length];
            System.Buffer.BlockCopy(_buffer, offset, newBuffer, 0, length);

            if (removeFromOriginal)
            {
                byte[] reBuffer = new byte[_buffer.Length - length];
                if (offset > 0) // Copy to Offset
                    System.Buffer.BlockCopy(_buffer, 0, reBuffer, 0, offset);
                if (_buffer.Length > offset + length)
                    System.Buffer.BlockCopy(_buffer, offset + length, reBuffer, offset, _buffer.Length - (offset + length));
                _buffer = reBuffer;
            }

            return new NetworkBuffer(newBuffer);
        }

        public void CopyTo(byte[] buffer, int offset, int length)
        {
            System.Buffer.BlockCopy(_buffer, 0, buffer, offset, length);
        }
    }
}
