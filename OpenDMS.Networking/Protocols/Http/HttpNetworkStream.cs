using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpNetworkStream : Stream
    {
        public enum DirectionType
        {
            None = 0,
            Upload = 1,
            Download = 2
        }

        public DirectionType Direction { get; private set; }

        public HttpNetworkStream(DirectionType direction)
        {
            Direction = direction;
        }

        public override bool CanRead
        {
            get 
            {
                if (Direction == DirectionType.Upload) return true;
                else return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (Direction == DirectionType.Upload) return true;
                else return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (Direction == DirectionType.Download) return true;
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
