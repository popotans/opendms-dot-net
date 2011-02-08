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
        /// <param name="generalLogger">A reference to the <see cref="Logger"/> that this instance should use to document general events.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> that this instance should use to document network events.</param>
        public GetHeadJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager, 
            FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, 
            ProgressMethodType.Indeterminate, errorManager, fileSystem, generalLogger, 
            networkLogger)
        {
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

            _currentState = State.Active | State.Executing;

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.TimeoutFailedToStart(e, this, "GetHeadJob"));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (_fullAsset.MetaAsset == null)
            {
                _errorManager.AddError(ErrorMessage.GetHeadFailedDueToInvalidState(this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            try
            {
                head = _fullAsset.GetHeadFromServer(this, _networkLogger);
                MD5 = head.MD5;
                ETag = head.ETag;
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.GetHeadFailed(e, this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

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
