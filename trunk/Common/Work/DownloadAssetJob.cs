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
    /// An implementation of <see cref="AssetJobBase"/> that downloads the asset to the host 
    /// saving it to disk.
    /// </summary>
    public class DownloadAssetJob : AssetJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadAssetJob"/> class.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="fullAsset">A reference to a <see cref="Data.FullAsset"/> for this job.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public DownloadAssetJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset,
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager,
            FileSystem.IO fileSystem)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, ProgressMethodType.Determinate,
            errorManager, fileSystem)
        {
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            _currentState = State.Active | State.Executing;

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.TimeoutFailedToStart(e, this, "DownloadAssetJob"));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (!_fullAsset.MetaAsset.DownloadFromServer(this))
            {
                _errorManager.AddError(ErrorMessage.JobRunFailed(null, this));
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

            UpdateLastAction();

            _fullAsset.DataAsset.OnProgress += new Data.DataAsset.ProgressHandler(Run_DataAsset_OnProgress);

            if (!_fullAsset.DataAsset.DownloadFromServer(this, _fullAsset.MetaAsset))
            {
                _errorManager.AddError(ErrorMessage.JobRunFailed(null, this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            UpdateLastAction();

            // Check for Error
            if (this.IsError || CheckForAbortAndUpdate())
            {
                _fullAsset.DataAsset.OnProgress -= Run_DataAsset_OnProgress;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

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
            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
        }
    }
}
