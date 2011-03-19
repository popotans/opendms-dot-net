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
    /// An implementation of <see cref="ResourceJobBase"/> that uploads the asset to the host and then 
    /// downloads the updated <see cref="Data.MetaAsset"/> saving it to disk.
    /// </summary>
    public class SaveResourceJob 
        : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveResourceJob"/> class.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="fullAsset">A reference to a <see cref="Data.FullAsset"/> for this job.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public SaveResourceJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset,
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager,
            FileSystem.IO fileSystem)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, ProgressMethodType.Determinate,
            errorManager, fileSystem)
        {
            Logger.General.Debug("SaveResourceJob instantiated on job id " + id.ToString() + ".");
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            Logger.General.Debug("SaveResourceJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("SaveResourceJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a SaveResourceJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("SaveResourceJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            _fullAsset.DataAsset.OnProgress += new Data.DataAsset.ProgressHandler(Run_DataAsset_OnProgress);

            Logger.General.Debug("Begining saving of the full asset on server for SaveResourceJob with id " + Id.ToString() + "."); 

            if (!_fullAsset.SaveToServer(this))
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.CreateAssetOnServerFailed,
                    "Saving of Asset Failed",
                    "I failed to save the asset on the remote server, for additional details consult the logs.",
                    "Failed to create the asset on the remote server for SaveResourceJob with id " + Id.ToString() + ", for additional details consult earlier log entries and log entries on the server.",
                    true, true);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Completed saving the full asset on server for SaveResourceJob with id " + Id.ToString() + ".");

            // No need to monitor this event anymore
            _fullAsset.DataAsset.OnProgress -= Run_DataAsset_OnProgress;

            if (IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Updating the local meta asset for SaveResourceJob with id " + Id.ToString() + ".");

            // Now we need to update the local meta asset - version #s, etags and such
            try
            {
                // Downloads it
                _fullAsset.MetaAsset.DownloadFromServer(this);
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.DownloadMetaAssetFailed,
                    "Updating Local Asset Failed",
                    "I failed to update the local asset's meta information, but I did successfully save the asset on the server.  Please update the local asset.",
                    "Failed to update the local asset's meta information for SaveResourceJob with id " + Id.ToString() + ", but the asset was successfully created on the server.",
                    true, true, e);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Updating the local meta asset for SaveResourceJob with id " + Id.ToString() + ", was successful.");

            _currentState = State.Active | State.Finished;
            _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
            return this;
        }

        /// <summary>
        /// Called when the <see cref="Data.DataAsset"/> portion of the <see cref="Data.FullAsset"/> 
        /// makes progress uploading.
        /// </summary>
        /// <param name="sender">A reference to the <see cref="Data.DataAsset"/> that made progress.</param>
        /// <param name="percentComplete">The percent complete.</param>
        void Run_DataAsset_OnProgress(Data.DataAsset sender, int percentComplete)
        {
            Logger.General.Debug("SaveResourceJob with id " + Id.ToString() + " is now " + percentComplete.ToString() + "% complete.");

            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
        }
    }
}
