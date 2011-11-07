using System;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class TcpConnectionAsyncEventArgs
    {
        private Timeout _timeout = null;

        public TcpConnectionAsyncEventArgs(Timeout timeout)
        {
            _timeout = timeout;
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
