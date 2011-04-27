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
    /// An implementation of <see cref="ResourceJobBase"/> that downloads the asset to the host 
    /// saving it to disk.
    /// </summary>
    public class GetResourceJob 
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
        /// <param name="requestingUser">The user requesting the action.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        /// <param name="couchdb">A reference to the <see cref="CouchDB.Database"/>.</param>
        public GetResourceJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Determinate;
            Logger.General.Debug("GetResourceJob instantiated on job id " + args.Id.ToString() + ".");
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

            Logger.General.Debug("GetResourceJob started on job id " + Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("GetResourceJob timeout is starting on job id " + Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                Logger.General.Error("Timeout failed to start on a GetResourceJob with id " + Id.ToString() + ".");
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a GetResourceJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("GetResourceJob timeout has started on job id " + Id.ToString() + ".");

            Logger.General.Debug("Begining meta asset download for GetResourceJob with id " + Id.ToString() + ".");

            if (!_jobResource.GetMetaAssetFromRemote(this, out errorMessage))
            {
                Logger.General.Error("Failed to download the asset's meta information for GetResourceJob with id " + 
                    Id.ToString() + " with error message: " + errorMessage);
                _errorManager.AddError(ErrorMessage.ErrorCode.DownloadMetaAssetFailed,
                    "Downloading Asset Failed",
                    "I failed to download the meta information.  Please try again.",
                    "Failed to download the asset's meta information for GetResourceJob with id " + Id.ToString() + ".",
                    true, true);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed the meta asset download for GetResourceJob with id " + Id.ToString() + ".");

            // Check for Error
            if (this.IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Saving meta asset to local filesystem.");

            if (!_jobResource.MetaAsset.SaveToLocal(this, _fileSystem))
            {
                Logger.General.Error("Failed to save the meta asset to the filesystem.");
                _errorManager.AddError(ErrorMessage.ErrorCode.DownloadMetaAssetFailed,
                    "Downloading Asset Failed",
                    "I failed to saved the meta information.  Please try again.",
                    "Failed to save the asset's meta information for GetResourceJob with id " + Id.ToString() + ".",
                    true, true);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed the meta asset save for GetResourceJob with id " + Id.ToString() + ".");

            // Check for Error
            if (this.IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            _jobResource.DataAsset.OnProgress += new Storage.DataAsset.ProgressHandler(Run_DataAsset_OnProgress);

            Logger.General.Debug("Begining data asset download for GetResourceJob with id " + Id.ToString() + ".");

            if (!_jobResource.DownloadDataAssetAndSaveLocally(this, _fileSystem, out errorMessage))
            {
                Logger.General.Error("Failed to download the asset's data information for GetResourceJob with id " +
                    Id.ToString() + " with error message: " + errorMessage);
                _errorManager.AddError(ErrorMessage.ErrorCode.DownloadDataAssetFailed,
                    "Downloading Asset Failed",
                    "I failed to download the data asset.  Please try again.",
                    "Failed to download the asset's data for GetResourceJob with id " + Id.ToString() + ".",
                    true, true);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed the data asset download for GetResourceJob with id " + Id.ToString() + ".");

            UpdateLastAction();

            // Check for Error
            if (this.IsError || CheckForAbortAndUpdate())
            {
                _jobResource.DataAsset.OnProgress -= Run_DataAsset_OnProgress;
                ReportWork(this);
                return this;
            }

            _currentState = State.Active | State.Finished;
            _jobResource.DataAsset.OnProgress -= Run_DataAsset_OnProgress;
            ReportWork(this);
            return this;
        }

        /// <summary>
        /// Called when the <see cref="Data.DataAsset"/> portion of the <see cref="Data.FullAsset"/> 
        /// makes progress downloading.
        /// </summary>
        /// <param name="sender">A reference to the <see cref="Data.DataAsset"/> that made progress.</param>
        /// <param name="percentComplete">The percent complete.</param>
        void Run_DataAsset_OnProgress(Storage.DataAsset sender, int percentComplete)
        {
            Logger.General.Debug("GetResourceJob with id " + Id.ToString() + " is now " + percentComplete.ToString() + "% complete.");

            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                ReportWork(this);
        }
    }
}
