using System;

namespace Common.Network
{
    public class NetworkException 
        : Exception
    {
        private State _state;
        public State WebState { get { return _state; } set { _state = value; } }

        public NetworkException(State state)
            : base()
        {
            _state = state;
        }

        public NetworkException(State state, string message)
            : base(message)
        {
            _state = state;
        }

        public NetworkException(State state, string message, Exception innerException)
            : base(message, innerException)
        {
            _state = state;
        }
    }
}
