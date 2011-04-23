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
    /// An implementation of <see cref="ResourceJobBase"/> that locks the asset 
    /// on the remote host.
    /// </summary>
    public class LockJob : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockJob"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public LockJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Indeterminate;
            Logger.General.Debug("LockJob instantiated on job id " + args.Id.ToString() + ".");
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

            Logger.General.Debug("LockJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("LockJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a LockJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("LockJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Begining formatting of network message for LockJob with id " + Id.ToString() + ".");

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    "_lock", _jobResource.MetaAsset.GuidString, Network.OperationType.PUT, Network.DataStreamMethod.Memory,
                    null, null, null, null, false, false, false, false,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while created the network message for LockJob with id " + Id.ToString() + ".", e);
                _errorManager.AddError(ErrorMessage.ErrorCode.LockAssetFailed,
                    "Locking of Asset Failed",
                    "I failed to lock the asset.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Failed to lock the asset for LockJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed formatting of network message for LockJob with id " + Id.ToString() + ".");

            Logger.General.Debug("Beginning locking of asset on server for LockJob with id " + Id.ToString() + ".");

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message for LockJob with id " + Id.ToString() + ".", e);
                _errorManager.AddError(ErrorMessage.ErrorCode.LockAssetFailed,
                   "Locking of Asset Failed",
                   "I failed to lock the asset.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                   "Failed to lock the asset for LockJob with id " + Id.ToString() + ".",
                   true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed locking of asset on server for LockJob with id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            sr = new NetworkPackage.ServerResponse();

            Logger.General.Debug("Beginning deserialization of the server response for LockJob with id " + Id.ToString() + ".");

            try
            {
                sr.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while calling NetworkPackage.ServerResponse.Deserialize() for LockJob with id " + Id.ToString() + ".", e);
                _errorManager.AddError(ErrorMessage.ErrorCode.LockAssetFailed,
                   "Locking of Asset Might Have Failed",
                   "I delivered the command to lock the asset, but I received a response back from the server which I did not understand, thus, I cannot guarantee that the change was made.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                   "Failed to deserialize the server response for LockJob with id " + Id.ToString() + ".",
                   true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed deserialization of the server response for LockJob with id " + Id.ToString() + ".");

            if (!(bool)sr["Pass"])
            {
                Logger.Network.Error("Failed to lock the resource.");
                _errorManager.AddError(ErrorMessage.ErrorCode.LockAssetFailed,
                   "Locking of Asset Failed",
                   "I delivered the command to lock the asset, but I received a response stating that it failed.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                   "Failed to lock the asset on the server for LockJob with id " + Id.ToString() + ".",
                   true, true);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed locking of the asset for LockJob with id " + Id.ToString() + ".");

            _currentState = State.Active | State.Finished;
            ReportWork(this);
            return this;
        }
    }
}
