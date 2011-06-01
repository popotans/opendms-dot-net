using System;

namespace Common.Http.Network
{
    public class AsyncUserToken
    {
        private object _object1 = null;
        private object _object2 = null;
        private Timeout _timeout = null;

        public object Token1 { get { return _object1; } }
        public object Token2 { get { return _object2; } }

        public AsyncUserToken(object obj1)
        {
            _object1 = obj1;
        }

        public AsyncUserToken(object obj1, object obj2)
        {
            _object1 = obj1;
            _object2 = obj2;
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
