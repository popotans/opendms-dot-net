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
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="resource">A reference to a <see cref="Storage.Resource"/> for this job.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        public CheckUpdateStatusJob(IWorkRequestor requestor, ulong id, Storage.Resource resource,
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager)
            : base(requestor, id, resource, actUpdateUI, timeout, ProgressMethodType.Determinate,
            errorManager)
        {
            Logger.General.Debug("CheckUpdateStatusJob instantiated on job id " + id.ToString() + ".");
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
                _requestor.WorkReport(_actUpdateUI, this, _jobResource);
                return this;
            }

            Logger.General.Debug("CheckUpdateStatusJob timeout has started on job id " + Id.ToString() + ".");

            Logger.General.Debug("Begining meta asset download for CheckUpdateStatusJob with id " + Id.ToString() + ".");

            if (!_jobResource.GetMetaAssetFromRemote(this, out errorMessage))
            {
                Logger.General.Error("Failed to download the asset's meta information for CheckUpdateStatusJob with id " +
                    Id.ToString() + " with error message: " + errorMessage);
                _errorManager.AddError(ErrorMessage.ErrorCode.DownloadMetaAssetFailed,
                    "Downloading Asset Failed",
                    "I failed to download the meta information.  Please try again.",
                    "Failed to download the asset's meta information for CheckUpdateStatusJob with id " + Id.ToString() + ".",
                    true, true);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _jobResource);
                return this;
            }

            Logger.General.Debug("Successfully completed the meta asset download for CheckUpdateStatusJob with id " + Id.ToString() + ".");

            UpdateLastAction();

            // Check for Error
            if (this.IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _jobResource);
                return this;
            }

            _currentState = State.Active | State.Finished;
            _requestor.WorkReport(_actUpdateUI, this, _jobResource);
            return this;
        }
    }
}
