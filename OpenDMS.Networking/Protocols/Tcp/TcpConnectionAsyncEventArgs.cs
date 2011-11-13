using System;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class TcpConnectionAsyncEventArgs
    {
        private Timeout _timeout = null;
        public System.IO.Stream Stream { get; private set; }

        public TcpConnectionAsyncEventArgs(Timeout timeout)
        {
            _timeout = timeout;
        }

        public TcpConnectionAsyncEventArgs(Timeout timeout, System.IO.Stream stream)
            : this(timeout)
        {
            Stream = stream;
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
