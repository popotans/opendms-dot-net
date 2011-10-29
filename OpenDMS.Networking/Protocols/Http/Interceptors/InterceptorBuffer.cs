using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public class InterceptorBuffer
    {
        private byte[] _buffer;

        public int Position { get; private set; }
        //public int Size { get; private set; }
        public int MaximumSize { get; private set; }

        public InterceptorBuffer(int maximumSize)
        {
            MaximumSize = maximumSize;
            _buffer = new byte[MaximumSize];
            Position = 0;
        }

        public void AppendBlock(byte[] buffer)
        {
            AppendBlock(buffer, buffer.Length);
        }

        public void AppendBlock(byte[] buffer, int length)
        {
            AppendBlock(buffer, 0, length);
        }

        public void AppendBlock(byte[] buffer, int srcOffset, int length)
        {
            Buffer.BlockCopy(buffer, srcOffset, _buffer, Position, length);
            Position += length - srcOffset;
        }

        public void InsertBlock(byte[] buffer, int position)
        {
            InsertBlock(buffer, position, buffer.Length);
        }

        public void InsertBlock(byte[] buffer, int position, int length)
        {
            InsertBlock(buffer, position, 0, length);
        }

        public void InsertBlock(byte[] buffer, int position, int srcOffset, int length)
        {
            if (Position + (length-srcOffset) > MaximumSize)
                throw new InternalBufferOverflowException();
            if (srcOffset < 0)
                throw new IndexOutOfRangeException();

            byte[] temp = new byte[Position - position - srcOffset];

            // Copy into temp
            Buffer.BlockCopy(_buffer, position, temp, 0, length - srcOffset);

            // Copy buffer into _buffer overwriting what was copied into temp
            Buffer.BlockCopy(buffer, srcOffset, _buffer, position, length - srcOffset);
            Position = position + length - srcOffset;

            // Copy temp back into buffer
            Buffer.BlockCopy(temp, 0, _buffer, Position, temp.Length);
            Position += temp.Length;
        }

        public MemoryStream MakeStream()
        {
            return new MemoryStream(_buffer, 0, Position, true, true);
        }
    }
}
