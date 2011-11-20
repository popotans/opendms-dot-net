using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public abstract class InterceptorBase 
        : IStreamInterceptor
    {
        protected HttpNetworkStream _inputStream;

        public virtual bool CanRead
        {
            get { return _inputStream.CanRead; }
        }

        public virtual bool CanWrite
        {
            get { return _inputStream.CanWrite; }
        }

        public virtual long Position
        {
            get { throw new NotImplementedException(); }
            protected set { throw new NotImplementedException(); }
        }

        public InterceptorBase(HttpNetworkStream inputStream)
        {
            _inputStream = inputStream;
        }

        public virtual int Read(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public virtual void Flush()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            // Notice that we neither close nor dispose our _inputStream
        }
    }
}
