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
    /// An implementation of <see cref="ResourceJobBase"/> that downloads the asset if the ETag on 
    /// the host is newer and saves the asset to a local resource (on disk).
    /// </summary>
    public class LoadResourceJob : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadResourceJob"/> class.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="fullAsset">A reference to a <see cref="Data.FullAsset"/> for this job.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public LoadResourceJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager,
            FileSystem.IO fileSystem)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, ProgressMethodType.Determinate, 
            errorManager, fileSystem)
        {
            Logger.General.Debug("LoadResourceJob instantiated on job id " + id.ToString() + ".");
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            Data.ETag remoteEtag;

            Logger.General.Debug("LoadResourceJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("LoadResourceJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a LoadResourceJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("LoadResourceJob timeout has started on job id " + Id.ToString() + ".");

            if (CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Begining getting of the etag on the server for LoadResourceJob with id " + Id.ToString() + "."); 

            try
            {
                remoteEtag = _fullAsset.GetETagFromServer(this);
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.GetETagFailed,
                    "Loading Asset Failed",
                    "I failed to check the status of the asset on the remote server, for additional details consult the logs.",
                    "Failed to get the etag of the asset on the remote server for LoadResourceJob with id " + Id.ToString() + ", for additional details consult earlier log entries and log entries on the server.",
                    true, true, e);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Successfully completed getting the etag for LoadResourceJob with id " + Id.ToString() + ".");

            UpdateLastAction();

            if (_fullAsset.MetaAsset.ETag.IsOlder(remoteEtag))
            {
                // Local is older -> Get it
                Logger.General.Debug("The local asset is older than the remote asset, thus downloading can begin.");

                if (CheckForAbortAndUpdate())
                {
                    _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                    return this;
                }

                Logger.General.Debug("Begining meta asset download for LoadResourceJob with id " + Id.ToString() + ".");

                // Getting Meta
                if (!_fullAsset.MetaAsset.DownloadFromServer(this))
                {
                    _errorManager.AddError(ErrorMessage.ErrorCode.DownloadMetaAssetFailed,
                        "Downloading Asset Failed",
                        "I failed to download the meta information.  Please try again.",
                        "Failed to download the asset's meta information for LoadResourceJob with id " + Id.ToString() + ".",
                        true, true);
                    _currentState = State.Error;
                    _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                    return this;
                }

                Logger.General.Debug("Successfully completed the meta asset download for LoadResourceJob with id " + Id.ToString() + ".");

                // Check for Error
                if (this.IsError || CheckForAbortAndUpdate())
                {
                    _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                    return this;
                }

                // Reform the DataAsset - so we have the extension from the downloaded resource
                _fullAsset.DataAsset = new Data.DataAsset(_fullAsset.MetaAsset, _fileSystem);

                UpdateLastAction();

                _fullAsset.DataAsset.OnProgress += new Data.DataAsset.ProgressHandler(Run_DataAsset_OnProgress);

                Logger.General.Debug("Begining data asset download for LoadResourceJob with id " + Id.ToString() + ".");

                // Getting Data
                if (!_fullAsset.DataAsset.DownloadFromServer(this, _fullAsset.MetaAsset))
                {
                    _errorManager.AddError(ErrorMessage.ErrorCode.DownloadDataAssetFailed,
                        "Downloading Asset Failed",
                        "I failed to download the data asset.  Please try again.",
                        "Failed to download the asset's data for LoadResourceJob with id " + Id.ToString() + ".",
                        true, true);
                    _currentState = State.Error;
                    _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                    return this;
                }

                // Check for Error
                if (this.IsError || CheckForAbortAndUpdate())
                {
                    _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                    return this;
                }

                Logger.General.Debug("Successfully completed the data asset download for LoadResourceJob with id " + Id.ToString() + ".");

                UpdateLastAction();
            }

            _fullAsset.Load(this);

            _currentState = State.Active | State.Finished;
            _fullAsset.DataAsset.OnProgress -= Run_DataAsset_OnProgress;
            _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
            return this;
        }

        /// <summary>
        /// Called when the <see cref="Data.DataAsset"/> portion of the <see cref="Data.FullAsset"/> 
        /// makes progress downloading.
        /// </summary>
        /// <param name="sender">A reference to the <see cref="Data.DataAsset"/> that made progress.</param>
        /// <param name="percentComplete">The percent complete.</param>
        void Run_DataAsset_OnProgress(Data.DataAsset sender, int percentComplete)
        {
            Logger.General.Debug("LoadResourceJob with id " + Id.ToString() + " is now " + percentComplete.ToString() + "% complete.");

            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            
            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
        }
    }
}
