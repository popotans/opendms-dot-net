using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public class InterceptorBuffer : IDisposable
    {
        private byte[] _buffer;

        public int Position { get; private set; }
        public int MaximumSize { get; private set; }

        public InterceptorBuffer(int maximumSize)
        {
            MaximumSize = maximumSize;
            _buffer = new byte[MaximumSize];
            Position = 0;
        }

        public InterceptorBuffer(byte[] buffer, int offset)
        {
            _buffer = new byte[buffer.Length - offset];
            MaximumSize = _buffer.Length;
            Position = MaximumSize;
            Buffer.BlockCopy(buffer, offset, _buffer, 0, _buffer.Length);
        }

        public InterceptorBuffer(InterceptorBuffer source)
        { // Deep Copy
            _buffer = new byte[source.MaximumSize];
            Position = source.Position;
            MaximumSize = source.MaximumSize;
            source.BlockCopy(_buffer, 0, source.Position);
        }

        public InterceptorBuffer(InterceptorBuffer source, int maximumSize)
        { // Deep Copy
            _buffer = new byte[maximumSize];
            if (source.Position > maximumSize)
            {
                Position = maximumSize;                
            }
            else
            {
                Position = source.Position;
            }
            source.BlockCopy(_buffer, 0, Position);
            MaximumSize = source.MaximumSize;
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

        public void AppendBlock(InterceptorBuffer buffer, int srcOffset, int length)
        {
            Buffer.BlockCopy(buffer._buffer, srcOffset, _buffer, Position, length);
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

        public void RemoveBlock(int length)
        {
            RemoveBlock(0, length);
        }

        public void RemoveBlock(int position, int length)
        {
            Buffer.BlockCopy(_buffer, position + length, _buffer, position, length);
            Position -= length;
        }

        public byte this[int index]
        {
            get { return _buffer[index]; }
            set { _buffer[index] = value; }
        }

        public int IndexOf(byte[] bytes)
        {
            bool fail = true;

            for (int i = 0; i < _buffer.Length; i++)
            {
                // If the buffer index has the same value as the current index of "bytes" then match
                if (_buffer[i] == bytes[0])
                {
                    // Scan down the _buffer to see if we have a match
                    for (int j = 1; j < bytes.Length; j++)
                    {
                        // If the buffer index at i (initial loc) + j (the current index we are looking at in bytes)
                        // do not match, then we know we do not have a match
                        if (_buffer[i + j] != bytes[j])
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

        public string GetString(int postition, int length, System.Text.Encoding encoding)
        {
            return encoding.GetString(_buffer, postition, length);
        }

        public void BlockCopy(byte[] dest, int destOffset, int length)
        {
            BlockCopy(0, dest, destOffset, length);
        }

        public void BlockCopy(int srcOffset, byte[] dest, int destOffset, int length)
        {
            if (srcOffset + length > Position)
                throw new IndexOutOfRangeException();

            Buffer.BlockCopy(_buffer, srcOffset, dest, destOffset, length);
        }

        public void BlockCopy(InterceptorBuffer dest, int length)
        {
            if (dest.Position + length > dest.MaximumSize)
                throw new IndexOutOfRangeException();

            BlockCopy(dest._buffer, dest.Position, length);
        }

        public void Zero(int length)
        {
            Zero(0, length);
        }

        public void Zero(int position, int length)
        {
            for (int i = position; i < length; i++)
                _buffer[i] = 0;
        }

        public void Expand(int newMaximumSize)
        {
            if (newMaximumSize < MaximumSize)
                throw new ArgumentException("The new maximum size is less than the current size.");

            byte[] newBuffer = new byte[newMaximumSize];
            BlockCopy(0, newBuffer, 0, Position);
            MaximumSize = newMaximumSize;

            _buffer = newBuffer;
        }

        public void Dispose()
        {
            _buffer = null;
        }
    }
}
