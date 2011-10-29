using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public abstract class HttpNetworkStreamInterceptorBase 
        : IStreamInterceptor
    {
        protected HttpNetworkStream _inputStream;

        public virtual bool CanRead
        {
            get { return _inputStream.CanRead; }
        }

        public virtual bool CanSeek
        {
            get { return _inputStream.CanSeek; }
        }

        public virtual bool CanWrite
        {
            get { return _inputStream.CanWrite; }
        }

        public abstract long Position
        {
            get { throw new NotImplementedException(); }
        }

        public HttpNetworkStreamInterceptorBase(HttpNetworkStream inputStream)
        {
            _inputStream = inputStream;
        }

        public abstract int Read(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public abstract void Write(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public abstract void Flush()
        {
            throw new NotImplementedException();
        }

        public abstract void Close()
        {
            throw new NotImplementedException();
        }

        public abstract void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
