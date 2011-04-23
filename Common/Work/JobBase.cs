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
    /// <summary>
    /// An abstract class that represents the base requirements for any inheriting class
    /// </summary>
    public abstract class JobBase
    {
        /// <summary>
        /// Represents the method that handles updating the UI.
        /// </summary>
        /// <param name="result">The <see cref="JobResult"/>.</param>
        public delegate void UpdateUIDelegate(JobResult result);
        /// <summary>
        /// Represents the method that handles the completion of a cancellation request.
        /// </summary>
        /// <param name="actUpdateUI">The method that handles updating the UI.</param>
        public delegate void CancelCompleteDelegate(UpdateUIDelegate actUpdateUI);

        /// <summary>
        /// An enumeration of possible states for a job.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// No state set.
            /// </summary>
            None = 0,
            /// <summary>
            /// The job is active.
            /// </summary>
            Active = 1,
            /// <summary>
            /// The job is executing.
            /// </summary>
            Executing = 2,
            /// <summary>
            /// The job is cancelled.
            /// </summary>
            Cancelled = 4,
            /// <summary>
            /// The job has encountered an error.
            /// </summary>
            Error = 8,
            /// <summary>
            /// The job has timed out.
            /// </summary>
            Timeout = 16,
            /// <summary>
            /// The job has finished.
            /// </summary>
            Finished = 32
        }

        /// <summary>
        /// An enumeration of methods of tracking progress.
        /// </summary>
        public enum ProgressMethodType
        {
            /// <summary>
            /// No progress method has been set.
            /// </summary>
            None = 0,
            /// <summary>
            /// The progress type is indeterminate.
            /// </summary>
            Indeterminate,
            /// <summary>
            /// The progress type is determinate.
            /// </summary>
            Determinate
        }

        /// <summary>
        /// The id of the job.
        /// </summary>
        private ulong _id;
        /// <summary>
        /// A timestamp indicating when this job will timeout.
        /// </summary>
        private DateTime _timeoutAt;
        /// <summary>
        /// A timestamp indicating when the last action on this job happened.
        /// </summary>
        private DateTime _lastActionAt;
        /// <summary>
        /// The quantity of bytes completed.
        /// </summary>
        private ulong _bytesComplete;
        /// <summary>
        /// The total quantity of bytes to transmit.
        /// </summary>
        private ulong _bytesTotal;
        /// <summary>
        /// The percentage of the job that is complete.
        /// </summary>
        private int _percentComplete;
        /// <summary>
        /// The current <see cref="State"/> of the job.
        /// </summary>
        protected State _currentState;
        /// <summary>
        /// The <see cref="ProgressMethodType"/> of the job.
        /// </summary>
        protected ProgressMethodType _progressMethod;
        /// <summary>
        /// The method called to update the UI.
        /// </summary>
        protected UpdateUIDelegate _actUpdateUI;
        /// <summary>
        /// The method that requested job execution.
        /// </summary>
        protected IWorkRequestor _requestor;
        /// <summary>
        /// The timeout duration.
        /// </summary>
        protected uint _timeout;
        /// <summary>
        /// A reference to the <see cref="ErrorManager"/>.
        /// </summary>
        protected ErrorManager _errorManager;
        /// <summary>
        /// A reference to the <see cref="FileSystem.IO"/>.
        /// </summary>
        protected FileSystem.IO _fileSystem;
        /// <summary>
        /// The user requesting the action.
        /// </summary>
        protected string _requestingUser;

        /// <summary>
        /// Gets the user requesting the action.
        /// </summary>
        public string RequestingUser { get { return _requestingUser; } }

        /// <summary>
        /// Gets the id of the job.
        /// </summary>
        public ulong Id { get { return _id; } }
        /// <summary>
        /// Gets the current <see cref="State"/> of the job.
        /// </summary>
        /// <value>
        /// The state of the job.
        /// </value>
        public State CurrentState { get { return _currentState; } }
        /// <summary>
        /// Gets the <see cref="ProgressMethodType"/> of the job.
        /// </summary>
        public ProgressMethodType ProgressMethod { get { return _progressMethod; } }
        /// <summary>
        /// Gets the quantity of bytes completed.
        /// </summary>
        public ulong BytesComplete { get { return _bytesComplete; } }
        /// <summary>
        /// Gets the total quantity of bytes to transmit.
        /// </summary>
        public ulong BytesTotal { get { return _bytesTotal; } }
        /// <summary>
        /// Gets the percentage of the job that is complete.
        /// </summary>
        public int PercentComplete { get { return _percentComplete; } }
        /// <summary>
        /// Gets the timestamp indicating when the last action on this job happened.
        /// </summary>
        public DateTime LastAction { get { return _lastActionAt; } }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get { return (_currentState & State.Active) == State.Active; } }
        /// <summary>
        /// Gets a value indicating whether this instance is executing.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is executing; otherwise, <c>false</c>.
        /// </value>
        public bool IsExecuting { get { return (_currentState & State.Executing) == State.Executing; } }
        /// <summary>
        /// Gets a value indicating whether this instance is cancelled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelled { get { return (_currentState & State.Cancelled) == State.Cancelled; } }
        /// <summary>
        /// Gets a value indicating whether this instance is error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError { get { return (_currentState & State.Error) == State.Error; } }
        /// <summary>
        /// Gets a value indicating whether this instance is timeout.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is timeout; otherwise, <c>false</c>.
        /// </value>
        public bool IsTimeout { get { return (_currentState & State.Timeout) == State.Timeout; } }
        /// <summary>
        /// Gets a value indicating whether this instance is finished.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is finished; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinished { get { return (_currentState & State.Finished) == State.Finished; } }
        /// <summary>
        /// Gets a value indicating whether the timeout thread can run.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the timeout thread can run; otherwise, <c>false</c>.
        /// </value>
        public bool TimeoutCanRun { get { return !IsCancelled && !IsTimeout && !IsFinished; } }
        /// <summary>
        /// Gets a value indicating whether this job should abort.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this job should abort; otherwise, <c>false</c>.
        /// </value>
        public bool AbortAction { get { return IsCancelled || IsTimeout; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobBase"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public JobBase(JobArgs args)
        {
            _id = args.Id;
            _actUpdateUI = args.UpdateUICallback;
            _timeout = args.Timeout;
            _requestingUser = args.RequestingUser;
            _currentState = State.None;
            _progressMethod = args.ProgressMethod;
            _errorManager = args.ErrorManager;
            _requestor = args.Requestor;
            _fileSystem = args.FileSystem;
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>A reference to this instance.</returns>
        public abstract JobBase Run();

        /// <summary>
        /// Starts the timeout thread.
        /// </summary>
        protected void StartTimeout()
        {
            Thread thread = new Thread(new ThreadStart(RunTimeout));
            thread.Start();
        }

        /// <summary>
        /// The timeout thread.
        /// </summary>
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

        /// <summary>
        /// Cancels this job.
        /// </summary>
        public void Cancel()
        {
            _currentState |= State.Cancelled;
        }

        /// <summary>
        /// Sets the error flag.
        /// </summary>
        public void SetErrorFlag()
        {
            _currentState |= State.Error;
        }

        /// <summary>
        /// Updates the last action and resets the timeout.
        /// </summary>
        public void UpdateLastAction()
        {
            _lastActionAt = DateTime.Now;
            _timeoutAt = DateTime.Now.AddMilliseconds(_timeout);
        }

        /// <summary>
        /// Updates the progress of the job.
        /// </summary>
        /// <param name="bytesComplete">The quantity of bytes completed.</param>
        /// <param name="bytesTotal">The total quantity of bytes to transmit.</param>
        /// <returns>An int representing the percentage of completion.</returns>
        public int UpdateProgress(ulong bytesComplete, ulong bytesTotal)
        {
            _bytesComplete = bytesComplete;
            _bytesTotal = bytesTotal;
            _percentComplete = (int)(((double)bytesComplete / (double)bytesTotal) * 100D);
            return _percentComplete;
        }

        /// <summary>
        /// Checks if the thread needs aborted and updates the current state if so.
        /// </summary>
        /// <returns><c>True</c> if the thread is to abort; otherwise, <c>false</c>.</returns>
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
