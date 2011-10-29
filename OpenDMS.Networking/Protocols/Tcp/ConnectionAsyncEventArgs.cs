using System;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class ConnectionAsyncEventArgs
    {
        private Timeout _timeout = null;

        public ConnectionAsyncEventArgs(Timeout timeout)
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
