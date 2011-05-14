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
    /// An implementation of <see cref="ResourceJobBase"/> that uploads the asset to the server, creating
    /// a new resource on the server and then downloads the updated <see cref="Data.MetaAsset"/> 
    /// saving it to disk.
    /// </summary>
    public class CreateResourceJob
        : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateResourceJob"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public CreateResourceJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Determinate;
            Logger.General.Debug("CreateResourceJob instantiated on job id " + args.Id.ToString() + ".");
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            FileSystem.MetaResource mr;
            FileSystem.DataResource dr;
            Common.Postgres.Version pgVersion;
            string errorMessage = null;

            Logger.General.Debug("CreateResourceJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("CreateResourceJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                Logger.General.Error("Timeout failed to start on a CreateResourceJob with id " + Id.ToString() + ".");
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a CreateResourceJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("CreateResourceJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            // Postgres work
            Postgres.Resource.CreateNewResource(RequestingUser, out pgVersion);

            // Rename files to the proper new GUID
            mr = new FileSystem.MetaResource(_jobResource.MetaAsset, _fileSystem);
            dr = new FileSystem.DataResource(_jobResource.DataAsset, _fileSystem);

            mr.DeleteFromFilesystem();
            dr.Rename(pgVersion.VersionGuid);

            // Assign the GUID received from Postgres to our internal objects
            Logger.General.Debug("Translating guid of " + _jobResource.MetaAsset.Guid.ToString("N") + " to " + pgVersion.VersionGuid.ToString("N"));
            _requestor.ServerTranslation(this, _jobResource.MetaAsset.Guid, pgVersion.VersionGuid);
            _jobResource.MetaAsset.Guid = _jobResource.DataAsset.Guid = pgVersion.VersionGuid;

            // Save MA
            _jobResource.MetaAsset.SaveToLocal(this, _fileSystem);

            _jobResource.DataAsset.OnUploadProgress += new Storage.DataAsset.ProgressHandler(DataAsset_OnUploadProgress);
            _jobResource.DataAsset.OnTimeout += new Storage.DataAsset.EventHandler(DataAsset_OnTimeout);

            Logger.General.Debug("Begining full asset creation on server for CreateResourceJob with id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            // Creates MA on server, deletes old MA, renames DA
            if (!_jobResource.CreateResourceOnRemote(this, _fileSystem, SettingsBase.Instance.NetworkBufferSize, 
                SettingsBase.Instance.NetworkBufferSize, out errorMessage))
            {
                if (!_currentState.HasFlag(State.Timeout))
                {
                    Logger.General.Error("Failed to create the resource for CreateResourceJob with id " +
                           Id.ToString() + " with error message: " + errorMessage);
                    if (!IsCancelled)
                    {
                        _errorManager.AddError(ErrorMessage.ErrorCode.CreateResourceOnServerFailed,
                            "Resource Creation Failed",
                            "I failed to create the resource on the remote server, for additional details consult the logs.",
                            "Failed to create the resource on the remote server for CreateResourceJob with id " + Id.ToString() + ", for additional details consult earlier log entries and log entries on the server.",
                            true, true);
                        _currentState = State.Error;
                    }
                }

                ReportWork(this);
                return this;
            }

            UpdateLastAction();

            Logger.General.Debug("Completed full asset creation on server for CreateResourceJob with id " + Id.ToString() + ".");

            // No need to monitor this event anymore
            _jobResource.DataAsset.OnUploadProgress -= DataAsset_OnUploadProgress;
            _jobResource.DataAsset.OnTimeout -= DataAsset_OnTimeout;

            Logger.General.Debug("Updating the local meta asset for CreateResourceJob with id " + Id.ToString() + ".");

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
            Logger.General.Debug("CreateResourceJob with id " + Id.ToString() + " is now " + PercentComplete.ToString() + "% complete.");

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                ReportWork(this);
        }
    }
}
