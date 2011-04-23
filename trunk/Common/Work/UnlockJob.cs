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
    /// An implementation of <see cref="ResourceJobBase"/> that unlocks the asset 
    /// on the remote host.
    /// </summary>
    public class UnlockJob : ResourceJobBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnlockJob"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public UnlockJob(JobArgs args)
            : base(args)
        {
            args.ProgressMethod = ProgressMethodType.Indeterminate;
            Logger.General.Debug("UnlockJob instantiated on job id " + args.Id.ToString() + ".");
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

            Logger.General.Debug("UnlockJob started on job id " + this.Id.ToString() + ".");

            _currentState = State.Active | State.Executing;

            Logger.General.Debug("UnlockJob timeout is starting on job id " + this.Id.ToString() + ".");

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.TimeoutFailedToStart,
                    "Timeout Failed to Start",
                    "I failed start an operation preventing system lockup when a process takes to long to complete.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Timeout failed to start on a UnlockJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("UnlockJob timeout has started on job id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Begining formatting of network message for UnlockJob with id " + Id.ToString() + ".");

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    "_lock", Resource.MetaAsset.GuidString+ "?releaselock=true", Network.OperationType.PUT, 
                    Network.DataStreamMethod.Memory,
                    null, null, null, null, false, false, false, false,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while created the network message for UnlockJob with id " + Id.ToString() + ".", e);
                _errorManager.AddError(ErrorMessage.ErrorCode.UnlockAssetFailed,
                    "Unlocking of Resource Failed",
                    "I failed to unlock the resource.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                    "Failed to unlock the resource for UnlockJob with id " + Id.ToString() + ".",
                    true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed formatting of network message for UnlockJob with id " + Id.ToString() + ".");

            Logger.General.Debug("Beginning unlocking of resource on server for UnlockJob with id " + Id.ToString() + ".");

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message for UnlockJob with id " + Id.ToString() + ".", e);
                _errorManager.AddError(ErrorMessage.ErrorCode.UnlockAssetFailed,
                   "Unlocking of Resource Failed",
                   "I failed to unlock the resource.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                   "Failed to unlock the resource for UnlockJob with id " + Id.ToString() + ".",
                   true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed unlocking of resource on server for UnlockJob with id " + Id.ToString() + ".");

            if (IsError || CheckForAbortAndUpdate())
            {
                ReportWork(this);
                return this;
            }

            sr = new NetworkPackage.ServerResponse();

            Logger.General.Debug("Beginning deserialization of the server response for UnlockJob with id " + Id.ToString() + ".");

            try
            {
                sr.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while calling NetworkPackage.ServerResponse.Deserialize() for UnlockJob with id " + Id.ToString() + ".", e);
                _errorManager.AddError(ErrorMessage.ErrorCode.UnlockAssetFailed,
                   "Unlocking of Asset Might Have Failed",
                   "I delivered the command to unlock the resource, but I received a response back from the server which I did not understand, thus, I cannot guarantee that the change was made.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                   "Failed to deserialize the server response for UnlockJob with id " + Id.ToString() + ".",
                   true, true, e);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed deserialization of the server response for UnlockJob with id " + Id.ToString() + ".");

            if (!(bool)sr["Pass"])
            {
                Logger.Network.Error("Failed to lock the resource.");
                _errorManager.AddError(ErrorMessage.ErrorCode.UnlockAssetFailed,
                   "Unlocking of Resource Failed",
                   "I delivered the command to unlock the resource, but I received a response stating that it failed.  I am going to stop trying to perform the action you requested.  You might have to retry the action.",
                   "Failed to unlock the asset on the server for UnlockJob with id " + Id.ToString() + ".",
                   true, true);
                _currentState = State.Error;
                ReportWork(this);
                return this;
            }

            Logger.General.Debug("Successfully completed unlocking of the resource for UnlockJob with id " + Id.ToString() + ".");

            _currentState = State.Active | State.Finished;
            ReportWork(this);
            return this;
        }
    }
}
