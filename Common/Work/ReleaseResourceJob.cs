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

namespace Common.Work
{
    /// <summary>
    /// An implementation of <see cref="ResourceJobBase"/> that unlocks the resource on the server.
    /// </summary>
    public class ReleaseResourceJob
        : ResourceJobBase
    {
        public ReleaseResourceJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = JobBase.ProgressMethodType.Indeterminate;
            Logger.General.Debug("ReleaseResourceJob instantiated on job id " + args.Id.ToString() + ".");
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            Logger.General.Debug("ReleaseResourceJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("ReleaseResourceJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                Logger.General.Error("Timeout failed to start on a ReleaseResourceJob with id " + Id.ToString() + ".");
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a ReleaseResourceJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("CreateResourceJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            _currentState = State.Active | State.Finished;
            ReportWork(this);
            return this;
        }
    }
}
