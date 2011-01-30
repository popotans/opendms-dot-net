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
    public class LoadResourceJob : AssetJobBase
    {
        public LoadResourceJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager,
            FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, ProgressMethodType.Determinate, 
            errorManager, fileSystem, generalLogger, networkLogger)
        {
        }

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

        void Run_DataAsset_OnProgress(Data.DataAsset sender, int percentComplete)
        {
            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            
            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
        }
    }
}
