/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading;

namespace Common.Work
{
    public abstract class JobBase
    {
        public delegate void UpdateUIDelegate(JobBase job, Data.FullAsset fullAsset);
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
        protected UpdateUIDelegate _actUpdateUI;
        protected IWorkRequestor _requestor;
        protected uint _timeout;
        protected ErrorManager _errorManager;
        protected FileSystem.IO _fileSystem;
        protected Logger _generalLogger;
        protected Logger _networkLogger;

        public ulong Id { get { return _id; } }
        public State CurrentState { get { return _currentState; } }
        public ProgressMethodType ProgressMethod { get { return _progressMethod; } }
        public ulong BytesComplete { get { return _bytesComplete; } }
        public ulong BytesTotal { get { return _bytesTotal; } }
        public int PercentComplete { get { return _percentComplete; } }
        public DateTime LastAction { get { return _lastActionAt; } }

        public bool IsActive { get { return (_currentState & State.Active) == State.Active; } }
        public bool IsExecuting { get { return (_currentState & State.Executing) == State.Executing; } }
        public bool IsCancelled { get { return (_currentState & State.Cancelled) == State.Cancelled; } }
        public bool IsError { get { return (_currentState & State.Error) == State.Error; } }
        public bool IsTimeout { get { return (_currentState & State.Timeout) == State.Timeout; } }
        public bool IsFinished { get { return (_currentState & State.Finished) == State.Finished; } }
        public bool TimeoutCanRun { get { return !IsCancelled && !IsTimeout && !IsFinished; } }
        public bool AbortAction { get { return IsCancelled || IsTimeout; } }

        public JobBase(IWorkRequestor requestor, ulong id, UpdateUIDelegate actUpdateUI, uint timeout,
            ProgressMethodType progressMethod, ErrorManager errorManager, FileSystem.IO fileSystem,
            Logger generalLogger, Logger networkLogger)
        {
            _id = id;
            _actUpdateUI = actUpdateUI;
            _timeout = timeout;
            _currentState = State.None;
            _progressMethod = progressMethod;
            _errorManager = errorManager;
            _requestor = requestor;
            _fileSystem = fileSystem;
            _generalLogger = generalLogger;
            _networkLogger = networkLogger;
        }

        public abstract JobBase Run();

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

        public void SetErrorFlag()
        {
            _currentState |= State.Error;
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
