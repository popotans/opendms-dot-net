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
    /// An implementation of <see cref="AssetJobBase"/> that downloads the asset if the ETag on 
    /// the host is newer and saves the asset to a local resource (on disk).
    /// </summary>
    public class LoadResourceJob : AssetJobBase
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
        /// <param name="generalLogger">A reference to the <see cref="Logger"/> that this instance should use to document general events.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> that this instance should use to document network events.</param>
        public LoadResourceJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager,
            FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, ProgressMethodType.Determinate, 
            errorManager, fileSystem, generalLogger, networkLogger)
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
            Data.ETag remoteEtag;

            _currentState = State.Active | State.Executing;
            
            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.TimeoutFailedToStart(e, this, "LoadResourceJob"));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            try
            {
                remoteEtag = _fullAsset.GetETagFromServer(this, _networkLogger);
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.GetETagFailed(e, this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            UpdateLastAction();

            if (_fullAsset.MetaAsset.ETag.IsOlder(remoteEtag))
            {
                // Local is older -> Get it

                if (CheckForAbortAndUpdate())
                {
                    _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                    return this;
                }

                // Getting Meta
                if (!_fullAsset.MetaAsset.DownloadFromServer(this, _networkLogger))
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

                // Reform the DataAsset - so we have the extension from the downloaded resource
                _fullAsset.DataAsset = new Data.DataAsset(_fullAsset.MetaAsset, _fileSystem, _generalLogger);

                UpdateLastAction();

                _fullAsset.DataAsset.OnProgress += new Data.DataAsset.ProgressHandler(Run_DataAsset_OnProgress);

                // Getting Data
                if (!_fullAsset.DataAsset.DownloadFromServer(this, _fullAsset.MetaAsset, _networkLogger))
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
            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            
            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
        }
    }
}
