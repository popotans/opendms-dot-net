using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public abstract class InterceptorBase 
        : IStreamInterceptor
    {
        public delegate void CompleteStringOperationDelegate(InterceptorBase sender, string result);
        public delegate void ErrorDelegate(InterceptorBase sender, string message, Exception exception);
        public delegate void ProgressDelegate(InterceptorBase sender, Tcp.DirectionType direction, int packetSize);
        public delegate void TimeoutDelegate(InterceptorBase sender);

        public event ErrorDelegate OnError;
        public event ProgressDelegate OnProgress;
        public event CompleteStringOperationDelegate OnStringOperationComplete;
        public event TimeoutDelegate OnTimeout;

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

        public virtual HttpNetworkStream HttpNetworkStream { get { return _inputStream; } }

        public InterceptorBase(HttpNetworkStream inputStream)
        {
            _inputStream = inputStream;
        }

        public virtual int Read(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public virtual void ReadAsync(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public virtual void ReadToEndAsync()
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

        protected void TriggerOnError(string message, Exception ex)
        {
            if (OnError != null) OnError(this, message, ex);
        }

        protected void TriggerOnProgress(Tcp.DirectionType direction, int packetSize)
        {
            if (OnProgress != null) OnProgress(this, direction, packetSize);
        }

        protected void TriggerOnTimeout()
        {
            if (OnTimeout != null) OnTimeout(this);
        }

        protected void TriggerOnComplete(string result)
        {
            if (OnStringOperationComplete != null) OnStringOperationComplete(this, result);
        }
    }
}
