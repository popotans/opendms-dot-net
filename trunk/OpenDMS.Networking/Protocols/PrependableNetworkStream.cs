using System;
using System.Net.Sockets;

namespace OpenDMS.Networking.Protocols
{
    public class PrependableNetworkStream : IDisposable
    {
        #region Fields (4)

        private NetworkBuffer _bufferToPrepend = null;
        private long _bufferToPrependPosition = 0;
        private Socket _socket = null;
        // Before you think about using a memorystream, I have considered it.
        // The problem is that as you write to the stream, the earlier bytes remain
        // thereby, flooding memory for large streams.
        private NetworkStream _stream = null;

        #endregion Fields

        #region Constructors (2)

        public PrependableNetworkStream(Socket socket, System.IO.FileAccess fileAccess, bool ownsSocket, byte[] bytesToPrepend)
            : this(socket, fileAccess, ownsSocket)
        {
            _bufferToPrepend = new NetworkBuffer(bytesToPrepend);
        }

        public PrependableNetworkStream(Socket socket, System.IO.FileAccess fileAccess, bool ownsSocket)
        {
            _stream = new NetworkStream(socket, fileAccess, ownsSocket);
        }

        #endregion Constructors

        #region Properties (10)

        public bool CanRead { get { return _stream.CanRead; } }

        public bool CanSeek { get { return _stream.CanSeek; } }

        public bool CanTimeout { get { return _stream.CanTimeout; } }

        public bool CanWrite { get { return _stream.CanWrite; } }

        public bool DataAvailable { get { return _stream.DataAvailable; } }

        public long Length
        {
            get
            {
                if (_bufferToPrepend != null)
                    return _stream.Length + _bufferToPrepend.Length;
                return _stream.Length;
            }
        }

        public long Position
        {
            get
            {
                return _stream.Position + _bufferToPrependPosition;
            }
            set
            {
                if (value > _bufferToPrepend.Length)
                {
                    _bufferToPrependPosition = _bufferToPrepend.Length;
                    _stream.Position = value - _bufferToPrependPosition;
                }
                else
                {
                    _bufferToPrependPosition = value;
                    _stream.Position = 0;
                }
            }
        }

        public int ReadTimeout { get { return _stream.ReadTimeout; } set { _stream.ReadTimeout = value; } }

        public Socket Socket { get { return _socket; } }

        public int WriteTimeout { get { return _stream.WriteTimeout; } set { _stream.WriteTimeout = value; } }

        #endregion Properties

        #region Methods (8)

        // Public Methods (7) 

        public void Close()
        {
            _stream.Close();
        }

        public void Dispose()
        {
            _stream.Dispose();
            _bufferToPrepend = null;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;

            // is our position within the _buffer?
            if (_bufferToPrepend != null &&
                _bufferToPrependPosition < _bufferToPrepend.Length)
            {
                // Close frame to remaining bytes of _buffer if < count
                bytesRead = _bufferToPrepend.Length - (int)_bufferToPrependPosition;
                if (bytesRead < count)
                {
                    // Blockcopy
                    _bufferToPrepend.CopyTo(buffer, offset, bytesRead);
                    _bufferToPrependPosition += bytesRead; // advance _buff pos
                }
                else
                {
                    // Blockcopy
                    _bufferToPrepend.CopyTo(buffer, offset, count);
                    _bufferToPrependPosition += bytesRead; // advance _buff pos
                    return count;
                }
            }

            // If we get to here, _buffer has been read and now _stream needs used

            // Finish up
            bytesRead += _stream.Read(buffer, bytesRead, count - bytesRead);
            return bytesRead;
        }

        public byte ReadByte()
        {
            byte retVal;

            // is our position within the _buffer?
            if (_bufferToPrepend != null &&
                _bufferToPrependPosition < _bufferToPrepend.Length)
            {
                retVal = _bufferToPrepend[_bufferToPrependPosition];
                _bufferToPrependPosition++;
                return retVal;
            }

            // If we get to here, _buffer has been read and now _stream needs used

            // Finish up
            return (byte)_stream.ReadByte();
        }

        public void ReadAsync(StreamAsyncEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int howManyBufferBytesToUse = 0;
            int howManyStreamBytesToUse = e.Count;

            if (_bufferToPrepend != null &&
                _bufferToPrependPosition < _bufferToPrepend.Length)
            {
                // If we get here, we need to do some reading from _buffer, but how much?

                // This gives the remaining length of _buffer (the available frame)
                howManyBufferBytesToUse = (int)((long)_bufferToPrepend.Length - _bufferToPrependPosition);
                // Trim the frame if it is larger than we wanted
                if (e.Count <= howManyBufferBytesToUse) howManyBufferBytesToUse = e.Count;
                // Whatever is left, give to stream
                howManyStreamBytesToUse = e.Count - howManyBufferBytesToUse;

                // Blockcopy it in
                _bufferToPrepend.CopyTo(buffer, e.Offset, howManyBufferBytesToUse);

                // Reset our buffer
                e.SetBuffer(buffer, e.Offset + howManyBufferBytesToUse, howManyStreamBytesToUse);

                if (howManyStreamBytesToUse <= 0)
                {
                    // This could be improved to always return this method before calling Complete
                    // but consumers should not be depending on that anyway.
                    e.Complete(e);
                    return;
                }
            }

            _stream.BeginRead(e.Buffer, e.Offset, e.Count, new AsyncCallback(ReadAsync_Callback), e);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            // Writing just passes through to _stream since it is block based anyway
            _stream.Write(buffer, offset, count);
        }

        public void WriteAsync(StreamAsyncEventArgs e)
        {
            _stream.BeginWrite(e.Buffer, e.Offset, e.Count, new AsyncCallback(WriteAsync_Callback), e);
        }

        public void WriteAsync_Callback(IAsyncResult result)
        {
            StreamAsyncEventArgs e = (StreamAsyncEventArgs)result.AsyncState;

            _stream.EndWrite(result);

            e.Complete(e);
        }
        // Private Methods (1) 

        private void ReadAsync_Callback(IAsyncResult result)
        {
            StreamAsyncEventArgs e = (StreamAsyncEventArgs)result.AsyncState;
            int bytesRead = 0;

            bytesRead = _stream.EndRead(result);
            e.SetBuffer(e.Buffer, 0, bytesRead);

            e.Complete(e);
        }

        #endregion Methods 
    }
}
