
namespace OpenDMS.Networking.Http
{
    class AsyncUserToken
    {
		#region Fields (3) 

        private NetworkBuffer _networkBuffer = null;
        private object _object = null;
        private Timeout _timeout = null;

		#endregion Fields 

		#region Constructors (2) 

        public AsyncUserToken(NetworkBuffer networkBuffer, object obj)
        {
            _networkBuffer = networkBuffer;
            _object = obj;
        }

        public AsyncUserToken(NetworkBuffer networkBuffer)
        {
            _networkBuffer = networkBuffer;
        }

		#endregion Constructors 

		#region Properties (2) 

        public NetworkBuffer NetworkBuffer { get { return _networkBuffer; } }

        public object Token { get { return _object; } }

		#endregion Properties 

		#region Methods (2) 

		// Public Methods (2) 

        public AsyncUserToken StartTimeout(int milliseconds, Timeout.TimeoutEvent onTimeout)
        {
            _timeout = new Timeout(milliseconds, onTimeout).Start();
            return this;
        }

        public void StopTimeout()
        {
            _timeout.Stop();
        }

		#endregion Methods 
    }
}
