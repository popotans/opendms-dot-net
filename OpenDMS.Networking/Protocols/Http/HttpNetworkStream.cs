using System;
using System.IO;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpNetworkStream : Stream
    {
        public enum Direction
        {
            None = 0,
            Upload = 1,
            Download = 2
        }

        private List<Interceptors.IStreamInterceptor> _interceptors;
        private Direction _direction;

        public HttpNetworkStream(Direction direction)
        {
            _interceptors = new List<Interceptors.IStreamInterceptor>();
            _direction = direction;
        }

        public override bool CanRead
        {
            get 
            {
                if (_direction == Direction.Upload) return true;
                else return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (_direction == Direction.Upload) return true;
                else return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_direction == Direction.Download) return true;
                else return false;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
                throw new ArgumentException("Offset must be 0");

            for (int i = 0; i < _interceptors.Count; i++)
            {
                // Need to watch for overflow and underflow
                // Probably should make an InterceptorChain class to handle this.
                _interceptors[i].Read(buffer, count);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
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
