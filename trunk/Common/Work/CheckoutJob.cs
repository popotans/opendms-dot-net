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
    /// An implementation of <see cref="ResourceJobBase"/> that gets a lock on a resource and then downloads
    /// it from the host saving it to disk.
    /// </summary>
    public class CheckoutJob
        : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckoutJob"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public CheckoutJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Determinate;
            Logger.General.Debug("CheckoutJob instantiated on job id " + args.Id.ToString() + ".");
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

            Logger.General.Debug("CheckoutJob started on job id " + Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("CheckoutJob timeout is starting on job id " + Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                Logger.General.Error("Timeout failed to start on a CheckoutJob with id " + Id.ToString() + ".");
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a CheckoutJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("CheckoutJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            Logger.General.Debug("Begining resource checkout for CheckoutJob with id " + Id.ToString() + ".");

            if (!_jobResource.CheckoutResource(this, _fileSystem, SettingsBase.Instance.NetworkBufferSize,
                SettingsBase.Instance.NetworkBufferSize, out errorMessage))
            {
                if (!_currentState.HasFlag(State.Timeout))
                {
                    Logger.General.Error("Failed to checkout the resource for CheckoutJob with id " +
                        Id.ToString() + " with error message: " + errorMessage);
                    _errorManager.AddError(ErrorMessage.ErrorCode.CheckoutResourceFailed,
                        "Resource Checkout Failed",
                        "I failed to checkout the resource.  Please try again.",
                        "Failed to checkout the resource for CheckoutJob with id " + Id.ToString() + ".",
                        true, true);
                    _currentState = State.Error;
                }

                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed the resource checkout for CheckoutJob with id " + Id.ToString() + ".");

            UpdateLastAction();

            // Check for Error
            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            _jobResource.DataAsset.OnDownloadProgress += new Storage.DataAsset.ProgressHandler(DataAsset_OnDownloadProgress);
            _jobResource.DataAsset.OnTimeout += new Storage.DataAsset.EventHandler(DataAsset_OnTimeout);

            Logger.General.Debug("Begining resource download for CheckoutJob with id " + Id.ToString() + ".");

            if (!_jobResource.DownloadResourceAndSaveLocally(this, _fileSystem,
                SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkBufferSize, out errorMessage))
            {
                if (!_currentState.HasFlag(State.Timeout))
                {
                    Logger.General.Error("Failed to download the resource for CheckoutJob with id " +
                        Id.ToString() + " with error message: " + errorMessage);
                    _errorManager.AddError(ErrorMessage.ErrorCode.CheckoutResourceFailed,
                        "Resource Checkout Failed",
                        "I failed to download the resource.  Please try again.",
                        "Failed to download the resource for CheckoutJob with id " + Id.ToString() + ".",
                        true, true);
                    _currentState = State.Error;
                }

                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            Logger.General.Debug("Successfully completed the resource download for CheckoutJob with id " + Id.ToString() + ".");

            // No need to monitor this event anymore
            _jobResource.DataAsset.OnDownloadProgress -= DataAsset_OnDownloadProgress;
            _jobResource.DataAsset.OnTimeout -= DataAsset_OnTimeout;

            _currentState = State.Active | State.Finished;
            ReportWork(this);
            return this;
        }

        void DataAsset_OnTimeout(Storage.DataAsset sender)
        {
            _currentState = State.Timeout;
        }

        void DataAsset_OnDownloadProgress(Storage.DataAsset sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            Logger.General.Debug("CheckoutJob with id " + Id.ToString() + " is now " + PercentComplete.ToString() + "% complete.");

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                ReportWork(this);
        }
    }
}
