using System;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public class ChunkedEncodingInterceptor 
        : HttpNetworkStreamInterceptorBase
    {
        public override long Position
        {
            get { throw new NotImplementedException(); }
        }

        public ChunkedEncodingInterceptor(HttpNetworkStream inputStream)
            : base(inputStream)
        {
        }

        public override int Read(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
