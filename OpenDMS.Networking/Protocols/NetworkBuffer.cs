using System;

namespace OpenDMS.Networking.Protocols
{
    public class NetworkBuffer
    {
        #region Fields (1)

        private byte[] _buffer = null;

        #endregion Fields

        #region Constructors (2)

        public NetworkBuffer(byte[] bytes)
        {
            _buffer = bytes;
        }

        public NetworkBuffer()
        {
        }

        #endregion Constructors

        #region Properties (3)

        public byte[] Buffer { get { return _buffer; } }

        public int Length { get { return _buffer.Length; } }

        public byte this[int index]
        {
            get { return _buffer[index]; }
            set { _buffer[index] = value; }
        }

        public byte this[long index]
        {
            get { return _buffer[index]; }
            set { _buffer[index] = value; }
        }

        #endregion Properties

        #region Methods (2)

        // Public Methods (2) 

        public void CopyTo(byte[] buffer, int offset, int length)
        {
            System.Buffer.BlockCopy(_buffer, 0, buffer, offset, length);
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

        #endregion Methods 
    }
}
