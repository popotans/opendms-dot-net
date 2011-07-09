
namespace OpenDMS.Networking.Http
{
    class AsyncUserToken
    {
		#region Fields (3) 

        private NetworkBuffer _networkBuffer = null;
        private object _object = null;
        private Timeout _timeout = null;
        private Methods.Request _request = null;

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

        public AsyncUserToken(Methods.Request request, NetworkBuffer networkBuffer, object obj)
        {
            _request = request;
            _networkBuffer = networkBuffer;
            _object = obj;
        }

        public AsyncUserToken(Methods.Request request, NetworkBuffer networkBuffer)
        {
            _request = request;
            _networkBuffer = networkBuffer;
        }

		#endregion Constructors 

		#region Properties (2) 

        public Methods.Request Request { get { return _request; } }

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
