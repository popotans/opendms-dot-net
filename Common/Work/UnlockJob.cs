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
    /// An implementation of <see cref="AssetJobBase"/> that unlocks the asset 
    /// on the remote host.
    /// </summary>
    public class UnlockJob : AssetJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnlockJob"/> class.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="fullAsset">A reference to a <see cref="Data.FullAsset"/> for this job.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public UnlockJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset,
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
            NetworkPackage.ServerResponse sr;
            Network.Message msg = null;
            _currentState = State.Active | State.Executing;

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.TimeoutFailedToStart(e, this, "UnlockJob"));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    "_lock", FullAsset.Guid.ToString("N") + "?releaselock=true", Network.OperationType.PUT, 
                    Network.DataStreamMethod.Memory,
                    null, null, null, null, false, false, false, false,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while creating the network message.", e);
                _errorManager.AddError(ErrorMessage.UnlockFailed(this,
                    "Please review the log file for details", "Review prior log entries for details."));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message.", e);
                _errorManager.AddError(ErrorMessage.UnlockFailed(this,
                    "Please review the log file for details", "Review prior log entries for details."));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            sr = new NetworkPackage.ServerResponse();

            try
            {
                sr.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while calling NetworkPackage.ServerResponse.Deserialize().", e);
                _errorManager.AddError(ErrorMessage.UnlockFailed(this,
                    "Please review the log file for details", "Review prior log entries for details."));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (!(bool)sr["Pass"])
            {
                Logger.Network.Error("Failed to unlock the resource.");
                _errorManager.AddError(ErrorMessage.LockFailed(this,
                    "Please review the log file for details", "Review prior log entries for details."));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            _currentState = State.Active | State.Finished;
            _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
            return this;
        }
    }
}
