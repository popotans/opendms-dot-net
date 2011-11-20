using System;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class TcpConnectionAsyncEventArgs
    {
        private Timeout _timeout = null;
        public System.IO.Stream Stream { get; private set; }
        public TcpConnection.AsyncCallback AsyncCallback { get; private set; }
        public int BytesTransferred { get; set; }
        public byte[] Buffer { get; set; }
        public int Length { get; set; }
        public object UserToken { get; set; }

        public TcpConnectionAsyncEventArgs(Timeout timeout)
        {
            _timeout = timeout;
        }

        public TcpConnectionAsyncEventArgs(Timeout timeout, TcpConnection.AsyncCallback callback)
            : this(timeout)
        {
            AsyncCallback = callback;
        }

        public TcpConnectionAsyncEventArgs(Timeout timeout, System.IO.Stream stream)
            : this(timeout)
        {
            Stream = stream;
        }

        public TcpConnectionAsyncEventArgs(Timeout timeout, System.IO.Stream stream, TcpConnection.AsyncCallback callback)
            : this(timeout, stream)
        {
            AsyncCallback = callback;
        }

        public void StartTimeout()
        {
            _timeout.Start();
        }

        public void StopTimeout()
        {
            _timeout.Stop();
        }
    }
}
