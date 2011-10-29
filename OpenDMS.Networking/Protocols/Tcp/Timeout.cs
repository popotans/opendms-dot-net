using System.Timers;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class Timeout
    {
        #region Fields (3)

        private int _milliseconds;
        private TimeoutEvent _onTimeout;
        private Timer _timer;

        #endregion Fields

        #region Constructors (2)

        public Timeout(int milliseconds, TimeoutEvent onTimeout)
            : this(milliseconds)
        {
            _onTimeout = onTimeout;
        }

        public Timeout(int milliseconds)
        {
            _milliseconds = milliseconds;
            _timer = new Timer(milliseconds);
            _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
        }

        #endregion Constructors

        #region Delegates and Events (2)

        // Delegates (1) 

        public delegate void TimeoutEvent();
        // Events (1) 

        public event TimeoutEvent OnTimeout;

        #endregion Delegates and Events

        #region Methods (4)

        // Public Methods (3) 

        public void Renew()
        {
            _timer.Stop();
            _timer.Start();
        }

        public Timeout Start()
        {
            _timer.Start();
            return this;
        }

        public void Stop()
        {
            _timer.Stop();
        }
        // Private Methods (1) 

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            if (_onTimeout != null) _onTimeout.Invoke();
            if (OnTimeout != null) OnTimeout();
        }

        #endregion Methods
    }
}
