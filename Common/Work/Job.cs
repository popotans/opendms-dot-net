using System;
using System.Threading;

namespace Common.Work
{
    public abstract class Job
    {
        public delegate void UpdateUIDelegate(Job job, Data.ResourceBase resource);
        public delegate void CancelCompleteDelegate(UpdateUIDelegate actUpdateUI);

        public enum State
        {
            None = 0,
            Active = 1,
            Executing = 2,
            Cancelled = 4,
            Error = 8,
            Timeout = 16,
            Finished = 32
        }

        public enum ProgressMethodType
        {
            None = 0,
            Indeterminate,
            Determinate
        }

        private ulong _id;
        private DateTime _timeoutAt;
        private DateTime _lastActionAt;
        private ulong _bytesComplete;
        private ulong _bytesTotal;
        private int _percentComplete;
        protected State _currentState;
        protected ProgressMethodType _progressMethod;
        protected Data.ResourceBase _resource;
        protected UpdateUIDelegate _actUpdateUI;
        protected IWorkRequestor _requestor;
        protected uint _timeout;
        protected ErrorManager _errorManager;

        public ulong Id { get { return _id; } }
        public State CurrentState { get { return _currentState; } }
        public ProgressMethodType ProgressMethod { get { return _progressMethod; } }
        public ulong BytesComplete { get { return _bytesComplete; } }
        public ulong BytesTotal { get { return _bytesTotal; } }
        public int PercentComplete { get { return _percentComplete; } }
        public Data.ResourceBase Resource { get { return _resource; } }
		public DateTime LastAction { get { return _lastActionAt; } }


        public bool IsActive { get { return (_currentState & State.Active) == State.Active; } }
        public bool IsExecuting { get { return (_currentState & State.Executing) == State.Executing; } }
        public bool IsCancelled { get { return (_currentState & State.Cancelled) == State.Cancelled; } }
        public bool IsError { get { return (_currentState & State.Error) == State.Error; } }
        public bool IsTimeout { get { return (_currentState & State.Timeout) == State.Timeout; } }
        public bool IsFinished { get { return (_currentState & State.Finished) == State.Finished; } }
        public bool TimeoutCanRun { get { return !IsCancelled && !IsTimeout && !IsFinished; } }
        public bool AbortAction { get { return IsCancelled || IsTimeout; } }

        public Job(IWorkRequestor requestor, ulong id, Data.ResourceBase resource, UpdateUIDelegate actUpdateUI, 
            uint timeout, ProgressMethodType progressMethod, ErrorManager errorManager)
        {
            _id = id;
            _resource = resource;
            _actUpdateUI = actUpdateUI;
            _timeout = timeout;
            _currentState = State.None;
            _progressMethod = progressMethod;
            _errorManager = errorManager;
            _requestor = requestor;
        }

        public abstract Job Run();

        protected void StartTimeout()
        {
            Thread thread = new Thread(new ThreadStart(RunTimeout));
            thread.Start();
        }

        private void RunTimeout()
        {
            _timeoutAt = DateTime.Now.AddMilliseconds(_timeout);
            while (TimeoutCanRun)
            {
                // If the last action is later than the timeout timestamp then timeout
                if (DateTime.Now >= _timeoutAt)
                {
                    _currentState |= State.Timeout;
                    break;
                }
                Thread.Sleep(500);
            }
        }

        public void Cancel()
        {
            _currentState |= State.Cancelled;
        }

        public void UpdateLastAction()
        {
            _lastActionAt = DateTime.Now;
            _timeoutAt = DateTime.Now.AddMilliseconds(_timeout);
        }

        public int UpdateProgress(ulong bytesComplete, ulong bytesTotal)
        {
            _bytesComplete = bytesComplete;
            _bytesTotal = bytesTotal;
            _percentComplete = (int)(((double)bytesComplete / (double)bytesTotal) * 100D);
            return _percentComplete;
        }

        public bool CheckForAbortAndUpdate()
        {
            if (AbortAction)
            {
                _currentState = _currentState & (State.Cancelled | State.Timeout);
                return true;
            }

            return false;
        }
    }
}
