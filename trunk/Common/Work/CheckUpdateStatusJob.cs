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
    public class CheckUpdateStatusJob 
        : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetResourceJob"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public CheckUpdateStatusJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Indeterminate;
            Logger.General.Debug("CheckUpdateStatusJob instantiated on job id " + args.Id.ToString() + ".");
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            string errorMessage;

            Logger.General.Debug("CheckUpdateStatusJob started on job id " + Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("CheckUpdateStatusJob timeout is starting on job id " + Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                Logger.General.Error("Timeout failed to start on a CheckUpdateStatusJob with id " + Id.ToString() + ".");
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a CheckUpdateStatusJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("CheckUpdateStatusJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            Logger.General.Debug("Begining meta asset download for CheckUpdateStatusJob with id " + Id.ToString() + ".");
            
            if (!_jobResource.GetMetaAssetFromRemote(this, SettingsBase.Instance.NetworkBufferSize,
                SettingsBase.Instance.NetworkBufferSize, out errorMessage))
            {
                if (!_currentState.HasFlag(State.Timeout))
                {
                    Logger.General.Error("Failed to download the asset's meta information for CheckUpdateStatusJob with id " +
                        Id.ToString() + " with error message: " + errorMessage);
                    _errorManager.AddError(ErrorMessage.ErrorCode.DownloadMetaAssetFailed,
                        "Downloading Asset Failed",
                        "I failed to download the meta information.  Please try again.",
                        "Failed to download the asset's meta information for CheckUpdateStatusJob with id " + Id.ToString() + ".",
                        true, true);
                    _currentState = State.Error;
                }

                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed the meta asset download for CheckUpdateStatusJob with id " + Id.ToString() + ".");

            UpdateLastAction();

            _currentState = State.Active | State.Finished;
            ReportWork(this);
            return this;
        }
    }
}
