using System;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public class ChunkedEncodingInterceptor 
        : InterceptorBase
    {
        private int _readChunkBytesRemaining = 0;

        public override long Position
        {
            get;
            protected set;
        }

        public ChunkedEncodingInterceptor(HttpNetworkStream inputStream)
            : base(inputStream)
        {
            Position = 0;
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            bool atEoS;
            int bytesRead = 0;
            int chunkLength = 0;
            int chunkLengthEnd = 0;
            byte[] eol = System.Text.Encoding.ASCII.GetBytes("\r\n");
            byte[] eof = System.Text.Encoding.ASCII.GetBytes("0\r\n");
            InterceptorBuffer bufferFromStream = new InterceptorBuffer(length);

            // Filling the bufferFromStream
            FillInternalBuffer(bufferFromStream, out atEoS);

            // bufferFromStream is now full or if its position < maximum size then it holds the end of the stream

            // Trim off \r\n at the start of the stream
            TrimStartEoL(bufferFromStream);

            if (_readChunkBytesRemaining <= 0) // Starting at Start of Chunk
            {
                // Now, the very first line is the chunk size followed by a line terminator \r\n
                // Remember, RFC2616 section 3.6.1 says this size is represented in HEX

                // This is where the end of the chunk
                chunkLengthEnd = bufferFromStream.IndexOf(eol);

                // This represents the length of the chunk
                chunkLength = Utilities.ConvertHexToInt(bufferFromStream.GetString(0, chunkLengthEnd, System.Text.Encoding.ASCII));
                
                // Remove the chunk-size
                bufferFromStream.RemoveBlock(chunkLengthEnd + 1);
            }
            else // Starting in Middle of Chunk
            {
                chunkLength = _readChunkBytesRemaining;
            }

            if ((bufferFromStream.Position < chunkLength) &&
                (bufferFromStream.Position < length))
            { // our internal buffer does not have enough data... why not?  Lets figure it out
                if (atEoS)
                { // We are at the end of the stream
                    _readChunkBytesRemaining = 0;
                    bufferFromStream.BlockCopy(buffer, offset, bufferFromStream.Position);
                    bufferFromStream.Zero(bufferFromStream.MaximumSize); // this is unnecessary, can be removed in the future
                    bytesRead = bufferFromStream.Position;
                }
                else
                { // For some reason we did not get enough from the network, lets ask for more
                    int oldSize = bufferFromStream.Position;

                    FillInternalBuffer(bufferFromStream, out atEoS);
                    
                    // Now lets see if we got more?
                    if (oldSize >= bufferFromStream.Position)
                        throw new HttpNetworkStreamException("Unable to read from stream.");
                    else // Increased so run again
                        Read(buffer, offset, length);
                }
            }
            else if (chunkLength < length)
            { // Handles reading the end of the chunk
                _readChunkBytesRemaining = 0; // Set flag saying we are at start of new chunk
                bufferFromStream.BlockCopy(buffer, offset, chunkLength); // copy to return buffer
                bufferFromStream.RemoveBlock(chunkLength + eol.Length); // remove copied and the following \r\n
                bytesRead += Read(buffer, chunkLength, length - chunkLength); // hit it again
            }
            else // length (amount requested) is < chunkLength (size of chunk)
            { // Handles reading start or middle of a chunk
                _readChunkBytesRemaining = chunkLength - length; // Set flag saying we are in the middle of a chunk
                bufferFromStream.BlockCopy(buffer, offset, length); // copy to return buffer
                bufferFromStream.RemoveBlock(length); // remove copied, we are not guaranteed to be EoL so, don't trim that
                bytesRead = length;
            }

            Position += bytesRead;
            return bytesRead;
        }

        private void TrimStartEoL(InterceptorBuffer buffer)
        {
            byte[] eol = System.Text.Encoding.ASCII.GetBytes("\r\n");

            // Trim off \r\n at the start of the stream
            while (buffer.IndexOf(eol) == 0)
            {
                buffer.RemoveBlock(0, eol.Length);
            }
        }

        public override void Write(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        private void FillInternalBuffer(InterceptorBuffer buffer, out bool atEndOfStream)
        {
            atEndOfStream = false;

            // Quick check to make sure we even need to hit the stream
            if (buffer.Position == buffer.MaximumSize)
                return;

            byte[] tempBuffer = new byte[buffer.MaximumSize - buffer.Position];
            int tempPos = 0;
            int bytesRead = 0;

            while ((bytesRead = _inputStream.Read(tempBuffer, tempPos, tempBuffer.Length - tempPos)) > 0)
            {
                tempPos += bytesRead;
                if (tempPos > tempBuffer.Length)
                    throw new IndexOutOfRangeException();
                else if (tempPos == tempBuffer.Length)
                    break;
            }

            if (bytesRead == 0)
                atEndOfStream = true;

            buffer.AppendBlock(tempBuffer, 0, tempPos);
        }
    }
}
