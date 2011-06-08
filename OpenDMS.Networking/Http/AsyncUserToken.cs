
namespace OpenDMS.Networking.Http
{
    class AsyncUserToken
    {
        private NetworkBuffer _networkBuffer = null;
        private object _object = null;
        private Timeout _timeout = null;

        public NetworkBuffer NetworkBuffer { get { return _networkBuffer; } }
        public object Token { get { return _object; } }

        public AsyncUserToken(NetworkBuffer networkBuffer)
        {
            _networkBuffer = networkBuffer;
        }

        public AsyncUserToken(NetworkBuffer networkBuffer, object obj)
        {
            _networkBuffer = networkBuffer;
            _object = obj;
        }

        public AsyncUserToken StartTimeout(int milliseconds, Timeout.TimeoutEvent onTimeout)
        {
            _timeout = new Timeout(milliseconds, onTimeout).Start();
            return this;
        }

        public void StopTimeout()
        {
            _timeout.Stop();
        }
    }
}
