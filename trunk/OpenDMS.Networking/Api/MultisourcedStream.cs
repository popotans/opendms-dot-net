using System;
using System.IO;
using System.Collections.Generic;

namespace OpenDMS.Networking.Api
{
    public class MultisourcedStream : Stream
    {
        private long _positionInStream;
        private int _currentStream;
        private long _overallPosition;
        private List<Stream> _streams;

        public override bool CanRead
        {
            get 
            {
                lock (_streams)
                {
                    for (int i = 0; i < _streams.Count; i++)
                        if (!_streams[i].CanRead)
                            return false;
                    return true;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                lock (_streams)
                {
                    for (int i = 0; i < _streams.Count; i++)
                        if (!_streams[i].CanSeek)
                            return false;
                    return true;
                }
            }
        }

        public override bool CanWrite
        {
            get
            {
                lock (_streams)
                {
                    for (int i = 0; i < _streams.Count; i++)
                        if (!_streams[i].CanWrite)
                            return false;
                    return true;
                }
            }
        }

        public override long Length
        {
            get
            {
                long l = 0;

                lock (_streams)
                {
                    for (int i = 0; i < _streams.Count; i++)
                        l += _streams[i].Length;
                    return l;
                }
            }
        }

        public override long Position
        {
            get
            {
                return _overallPosition;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public MultisourcedStream(params Stream[] list)
        {
            _positionInStream = _overallPosition = _overallPosition = 0;
            _streams = new List<Stream>();
            for (int i = 0; i < list.Length; i++)
                _streams.Add(list[i]);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Get us to the starting position
            Seek(offset, SeekOrigin.Current);
            
            int retVal = 0;
            int totalRetVal = 0;
            bool run = true;
            byte[] tempBuffer = new byte[count];

            while(run)
            {
                if (_currentStream >= _streams.Count)
                    break;

                // Read up to count from the current stream
                if (_streams[_currentStream].Length > _positionInStream + count)
                {
                    retVal = _streams[_currentStream].Read(tempBuffer, 0, count);
                    // Set the position in the current stream
                    _positionInStream += count;
                    run = false;
                }
                else
                {
                    retVal = _streams[_currentStream].Read(tempBuffer, 0, 
                        (int)(_streams[_currentStream].Length - _positionInStream));
                    // Increment the current stream
                    _currentStream++;
                    // Set the position in the current stream
                    _positionInStream = 0;
                }

                Buffer.BlockCopy(tempBuffer, 0, buffer, totalRetVal, retVal);

                // increase the total counter
                totalRetVal += retVal;

            }
            _overallPosition += totalRetVal;
            return totalRetVal;
        }

        public string ReadToEnd()
        {
            int bytesRead = 0;
            byte[] buffer = new byte[8192];
            string output = "";

            while ((bytesRead = Read(buffer, 0, buffer.Length)) > 0)
            {
                output += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }

            return output;
        }

        public void WriteToFile(string filepath)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[8192];
            System.IO.FileStream fs = new System.IO.FileStream(filepath, FileMode.Create);
            System.IO.BinaryWriter bw = new BinaryWriter(fs);

            while ((bytesRead = Read(buffer, 0, buffer.Length)) > 0)
            {
                bw.Write(buffer, 0, bytesRead);
            }

            bw.Flush();
            bw.Close();
            bw.Dispose();
        }

        private void ResetToPosition(long overallPosition)
        {
            lock (_streams)
            {
                long overallPositionAtStartOfStream = 0;
                long ourCurrentPosition = 0;
                int newCurrentStream = -1;

                for (int i = 0; i < _streams.Count; i++)
                {
                    // If the desired position is within the current stream
                    if (overallPositionAtStartOfStream + _streams[i].Length >= overallPosition)
                    {
                        // Set ourCurrentPosition
                        ourCurrentPosition = overallPosition - ourCurrentPosition;
                        newCurrentStream = i;
                        break;
                    }

                    // Add the length of the stream to the overallPositionAtStartOfStream
                    overallPositionAtStartOfStream += _streams[i].Length;
                }

                if (newCurrentStream < 0)
                    throw new IndexOutOfRangeException();

                _positionInStream = ourCurrentPosition;
                _currentStream = newCurrentStream;
                _streams[_currentStream].Position = _positionInStream;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    lock(_streams)
                    {
                        if (offset == 0)
                            return _overallPosition;
                        else
                        {
                            ResetToPosition(_currentStream + offset);
                            return _overallPosition;
                        }
                    }
                case SeekOrigin.End:
                    // This is entirely too I/O taxing, only implement if required.
                    throw new NotImplementedException("SeekOrigin.End not supported");
                default:
                    lock (_streams)
                    {
                        if (offset == 0)
                            return _overallPosition;
                        else
                        {
                            ResetToPosition(_currentStream + offset);
                            return _overallPosition;
                        }
                    }
                    //long cumulativeLength = 0;

                    //if (offset < 0)
                    //    throw new IndexOutOfRangeException();

                    //lock (_streams)
                    //{
                    //    // Cycling streams
                    //    for (int i = 0; i < _streams.Count; i++)
                    //    {
                    //        if (offset < cumulativeLength + _streams[i].Length)
                    //            cumulativeLength += _streams[i].Length;
                    //        else
                    //        {
                    //            _currentStream = i;
                    //            _positionInStream = offset - cumulativeLength;
                    //            _overallPosition = offset;
                    //            _streams[_currentStream].Position = _positionInStream;
                    //            return _overallPosition;
                    //        }
                    //    }
                    //    throw new IndexOutOfRangeException();
                    //}
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
