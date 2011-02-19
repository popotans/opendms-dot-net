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
    /// An implementation of <see cref="AssetJobBase"/> that gets the ETag and MD5 of the asset 
    /// from the remote host.
    /// </summary>
    public class GetHeadJob : AssetJobBase
    {
        /// <summary>
        /// The remote MD5 representation.
        /// </summary>
        /// <value>
        /// The string representation.
        /// </value>
        public string MD5 { get; set; }
        /// <summary>
        /// The remote <see cref="Data.ETag"/>.
        /// </summary>
        public Data.ETag ETag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetETagJob"/> class.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="fullAsset">A reference to a <see cref="Data.FullAsset"/> for this job.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public GetHeadJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager, 
            FileSystem.IO fileSystem)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, 
            ProgressMethodType.Indeterminate, errorManager, fileSystem)
        {
            Logger.General.Debug("GetHeadJob instantiated on job id " + id.ToString() + ".");
            ETag = null;
            MD5 = null;
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <returns>
        /// A reference to this instance.
        /// </returns>
        public override JobBase Run()
        {
            Data.Head head;

            Logger.General.Debug("GetHeadJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("GetHeadJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a GetHeadJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("GetHeadJob timeout has started on job id " + Id.ToString() + ".");

            if (CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (_fullAsset.MetaAsset == null)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.InvalidState,
                    "Cannot Check Status",
                    "I cannot check the status of the asset at this time.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "GetHeadJob failed because the meta asset property of the full asset is null with id " + Id.ToString() + ".",
                    true, true);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Begining getting of the head on server for GetHeadJob with id " + Id.ToString() + "."); 

            try
            {
                head = _fullAsset.GetHeadFromServer(this);
                MD5 = head.MD5;
                ETag = head.ETag;
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.GetHeadFailed,
                    "Check Asset Status Failed",
                    "I failed to check the status of the asset on the remote server, for additional details consult the logs.",
                    "Failed to get the etag of the asset on the remote server for GetHeadJob with id " + Id.ToString() + ", for additional details consult earlier log entries and log entries on the server.",
                    true, true, e);
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            Logger.General.Debug("Successfully completed getting the head for GetHeadJob with id " + Id.ToString() + ".");

            if (this.IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            _currentState = State.Active | State.Finished;
            _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
            return this;
        }
    }
}
