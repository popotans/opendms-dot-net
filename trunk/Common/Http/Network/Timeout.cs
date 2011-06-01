using System;
using System.Timers;

namespace Common.Http.Network
{
    public class Timeout
    {
        public delegate void TimeoutEvent();
        public event TimeoutEvent OnTimeout;

        private int _milliseconds;
        private Timer _timer;
        private TimeoutEvent _onTimeout;

        public Timeout(int milliseconds)
        {
            _milliseconds = milliseconds;
            _timer = new Timer(milliseconds);
            _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
        }

        public Timeout(int milliseconds, TimeoutEvent onTimeout)
            : this(milliseconds)
        {
            _onTimeout = onTimeout;
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

        public void Renew()
        {
            _timer.Stop();
            _timer.Start();
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            if (_onTimeout != null)_onTimeout.Invoke();
            if (OnTimeout != null) OnTimeout();
        }
    }
}
