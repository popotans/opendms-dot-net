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
    /// An implementation of <see cref="ResourceJobBase"/> that uploads the asset to the host.
    /// </summary>
    public class SaveResourceJob 
        : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveResourceJob"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public SaveResourceJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Determinate;
            Logger.General.Debug("SaveResourceJob instantiated on job id " + args.Id.ToString() + ".");
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            string errorMessage = null;
            
            Logger.General.Debug("SaveResourceJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("SaveResourceJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                Logger.General.Error("Timeout failed to start on a SaveResourceJob with id " + Id.ToString() + ".");
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a SaveResourceJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("SaveResourceJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            _jobResource.DataAsset.OnUploadProgress += new Storage.DataAsset.ProgressHandler(DataAsset_OnUploadProgress);
            _jobResource.DataAsset.OnTimeout += new Storage.DataAsset.EventHandler(DataAsset_OnTimeout);

            UpdateLastAction();

            Logger.General.Debug("Begining saving of the resource on server for SaveResourceJob with id " + Id.ToString() + "."); 

            if (!_jobResource.UpdateResourceOnRemote(this, _fileSystem, SettingsBase.Instance.NetworkBufferSize, 
                SettingsBase.Instance.NetworkBufferSize, out errorMessage))
            {
                if (!_currentState.HasFlag(State.Timeout))
                {
                    Logger.General.Error("Failed to create the resource for SaveResourceJob with id " +
                        Id.ToString() + " with error message: " + errorMessage);
                    _errorManager.AddError(ErrorMessage.ErrorCode.CreateResourceOnServerFailed,
                        "Saving of Resource Failed",
                        "I failed to save the resource on the remote server, for additional details consult the logs.",
                        "Failed to update the resource on the remote server for SaveResourceJob with id " + Id.ToString() + ", for additional details consult earlier log entries and log entries on the server.",
                        true, true);
                    _currentState = State.Error;
                }

                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            Logger.General.Debug("Completed saving the resource on server for SaveResourceJob with id " + Id.ToString() + ".");

            // No need to monitor this event anymore
            _jobResource.DataAsset.OnUploadProgress -= DataAsset_OnUploadProgress;
            _jobResource.DataAsset.OnTimeout -= DataAsset_OnTimeout;

            _currentState = State.Active | State.Finished;
            ReportWork(this);
            return this;
        }

        void DataAsset_OnTimeout(Storage.DataAsset sender)
        {
            _currentState = State.Timeout;
        }

        void DataAsset_OnUploadProgress(Storage.DataAsset sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            Logger.General.Debug("SaveResourceJob with id " + Id.ToString() + " is now " + PercentComplete.ToString() + "% complete.");

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                ReportWork(this);
        }
    }
}
