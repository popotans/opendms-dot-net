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
using System.Collections.Generic;

namespace WindowsClient
{
    /// <summary>
    /// Represents the state of a TreeViewItem in the main window.
    /// </summary>
    public class TVIState
    {
        /// <summary>
        /// An enumeration of event flag types.
        /// </summary>
        public enum EventFlagType
        {
            /// <summary>
            /// None set.
            /// </summary>
            None = 0,
            /// <summary>
            /// The asset is loading.
            /// </summary>
            Loading = 1,
            /// <summary>
            /// The asset is loaded.
            /// </summary>
            Loaded = 2,
            /// <summary>
            /// The asset action is canceled.
            /// </summary>
            Canceled = 4,
            /// <summary>
            /// The asset action has timed-out.
            /// </summary>
            Timeout = 8,
            /// <summary>
            /// The asset action has errored.
            /// </summary>
            Error = 16
        }

        /// <summary>
        /// An enumeration of resource status types.
        /// </summary>
        public enum ResourceStatusType
        {
            /// <summary>
            /// None set.
            /// </summary>
            None = 0,
            /// <summary>
            /// The local asset/resource is newer than the remote asset/resource.
            /// </summary>
            LocalIsNewer = 1,
            /// <summary>
            /// The local asset/resource is older than the remote asset/resource.
            /// </summary>
            LocalIsOlder = 2,
            /// <summary>
            /// The local asset/resource is the same age (version) as the remote.
            /// </summary>
            LocalMatchesRemote = 4,
            /// <summary>
            /// The remote asset/resource exists.
            /// </summary>
            /// <remarks>Beware that this data is not accurate unless RemoteExistsIsKnown is <c>true</c>.</remarks>
            RemoteExists = 8,
            /// <summary>
            /// The vaule contained in RemoteExists is accurate.
            /// </summary>
            RemoteExistsIsKnown = 16,
            /// <summary>
            /// The local asset/resource exists.
            /// </summary>
            /// <remarks>Beware that this data is not accurate unless LocalExistsIsKnown is <c>true</c>.</remarks>
            LocalExists = 32,
            /// <summary>
            /// The value contained in LocalExists is accurate.
            /// </summary>
            LocalExistsIsKnown = 64
        }

        /// <summary>
        /// The event flags for this instance.
        /// </summary>
        private EventFlagType _eventFlags;
        /// <summary>
        /// The resource status flags for this instance.
        /// </summary>
        private ResourceStatusType _resourceStatusFlags;

        /// <summary>
        /// Gets or sets the <see cref="Common.Storage.Resource"/> for this instance.
        /// </summary>
        /// <value>
        /// The <see cref="Common.Storage.Resource"/>.
        /// </value>
        public Common.Storage.Resource Resource { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading { get { return HasFlags(EventFlagType.Loading); } }
        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get { return HasFlags(EventFlagType.Loaded); } }
        /// <summary>
        /// Gets a value indicating whether this instance is canceled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is canceled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCanceled { get { return HasFlags(EventFlagType.Canceled); } }
        /// <summary>
        /// Gets a value indicating whether this instance has timed-out.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has timed-out; otherwise, <c>false</c>.
        /// </value>
        public bool IsTimeout { get { return HasFlags(EventFlagType.Timeout); } }
        /// <summary>
        /// Gets a value indicating whether this instance has errored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has errored; otherwise, <c>false</c>.
        /// </value>
        public bool IsError { get { return HasFlags(EventFlagType.Error); } }

        /// <summary>
        /// Gets a value indicating whether the local asset/resource is newer.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the local asset/resource is newer; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocalNewer { get { return HasFlags(ResourceStatusType.LocalIsNewer); } }
        /// <summary>
        /// Gets a value indicating whether the local asset/resource is older.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the local asset/resource is older; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocalOlder { get { return HasFlags(ResourceStatusType.LocalIsOlder); } }
        /// <summary>
        /// Gets a value indicating whether the local asset/resource is the same age (version) as the remote asset/resource.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the local asset/resource is the same age (version) as the remote asset/resource; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocalSameAsRemote { get { return HasFlags(ResourceStatusType.LocalMatchesRemote); } }
        /// <summary>
        /// Gets a value indicating whether the remote asset/resource exists.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the remote asset/resource exists; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Only use the value contained herein if <see cref="IsRemoteExistantKnown"/> is <c>true</c> because this data is 
        /// only accurate at that time.</remarks>
        public bool IsRemoteExistant { get { return HasFlags(ResourceStatusType.RemoteExists); } }
        /// <summary>
        /// Gets a value indicating whether <see cref="IsRemoteExistant"/> is accurate.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if <see cref="IsRemoteExistant"/> is accurate; otherwise, <c>false</c>.
        /// </value>
        public bool IsRemoteExistantKnown { get { return HasFlags(ResourceStatusType.RemoteExistsIsKnown); } }
        /// <summary>
        /// Gets a value indicating whether the local asset/resource exists.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the local asset/resource exists; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocalExistant { get { return HasFlags(ResourceStatusType.LocalExists); } }
        /// <summary>
        /// Gets a value indicating whether <see cref="IsLocalExistant"/> is accurate.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if <see cref="IsLocalExistant"/> is accurate; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocalExistantKnown { get { return HasFlags(ResourceStatusType.LocalExistsIsKnown); } }

        /// <summary>
        /// Gets a value indicating whether this instance can save.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can save; otherwise, <c>false</c>.
        /// </value>
        public bool CanSave { get { return IsLocalExistantKnown && IsLocalExistant && IsLocalNewer; } }
        /// <summary>
        /// Gets a value indicating whether this instance can get (download).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can get (download); otherwise, <c>false</c>.
        /// </value>
        public bool CanGet { get { return IsRemoteExistantKnown && IsRemoteExistant && (IsLocalOlder || IsLocalSameAsRemote); } }


        /// <summary>
        /// Initializes a new instance of the <see cref="TVIState"/> class.
        /// </summary>
        /// <param name="fullAsset">The <see cref="Common.Storage.Resource"/>.</param>
        public TVIState(Common.Storage.Resource resource)
        {
            _eventFlags = EventFlagType.None;
            _resourceStatusFlags = ResourceStatusType.None;
            Resource = resource;
        }

        /// <summary>
        /// Determines whether this instance has the specified event flag(s).
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns>
        ///   <c>true</c> if this instance has the specified event flag(s); otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>Multiple flags can be specified; however, this will only return <c>true</c> in the event that they all match.</remarks>
        private bool HasFlags(EventFlagType flag)
        {
            return (_eventFlags & flag) == flag;
        }

        /// <summary>
        /// Determines whether this instance has the specified resource status flag(s).
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <returns>
        ///   <c>true</c> if this instance has the specified resource status flag(s); otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>Multiple flags can be specified; however, this will only return <c>true</c> in the event that they all match.</remarks>
        private bool HasFlags(ResourceStatusType flag)
        {
            return (_resourceStatusFlags & flag) == flag;
        }

        /// <summary>
        /// Gets the event flags.
        /// </summary>
        /// <returns></returns>
        public EventFlagType GetEventFlags()
        {
            return _eventFlags;
        }

        /// <summary>
        /// Gets the resource status flags.
        /// </summary>
        /// <returns></returns>
        public ResourceStatusType GetResourceStatusFlags()
        {
            return _resourceStatusFlags;
        }

        /// <summary>
        /// Sets the event flags.
        /// </summary>
        /// <param name="flags">The event flags.</param>
        public void SetFlags(EventFlagType flags)
        {
            _eventFlags = flags;
        }

        /// <summary>
        /// Sets the resource status flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void SetFlags(ResourceStatusType flags)
        {
            _resourceStatusFlags = flags;
        }

        /// <summary>
        /// Clears the specified resource status flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void ClearFlags(ResourceStatusType flags)
        {
            _resourceStatusFlags &= ~flags;
        }

        /// <summary>
        /// Updates the event flags.
        /// </summary>
        /// <param name="isLoading">if set to <c>true</c> is loading.</param>
        /// <param name="isLoaded">if set to <c>true</c> is loaded.</param>
        /// <param name="isCanceled">if set to <c>true</c> is canceled.</param>
        /// <param name="isTimeout">if set to <c>true</c> has timed-out.</param>
        /// <param name="isError">if set to <c>true</c> has errored.</param>
        public void UpdateEvent(bool isLoading, bool isLoaded, bool isCanceled,
            bool isTimeout, bool isError)
        {
            if (isLoaded) _eventFlags |= EventFlagType.Loaded;
            else _eventFlags &= ~EventFlagType.Loaded;

            if (isLoading) _eventFlags |= EventFlagType.Loading;
            else _eventFlags &= ~EventFlagType.Loading;

            if (isCanceled) _eventFlags |= EventFlagType.Canceled;
            else _eventFlags &= ~EventFlagType.Canceled;

            if (isTimeout) _eventFlags |= EventFlagType.Timeout;
            else _eventFlags &= ~EventFlagType.Timeout;

            if (isError) _eventFlags |= EventFlagType.Error;
            else _eventFlags &= ~EventFlagType.Error;
        }

        /// <summary>
        /// Updates the resource status flags.
        /// </summary>
        /// <param name="localIsNewer">The local asset/resource is newer than the remote.</param>
        /// <param name="localIsOlder">The local asset/resource is older than the remote.</param>
        /// <param name="localMatchesRemote">The local asset/resource is the same age (version) as the remote.</param>
        /// <param name="remoteExists">The remote asset/resource exists.</param>
        /// <param name="localExists">The local asset/resource exists.</param>
        public void UpdateResourceStatus(bool? localIsNewer, bool? localIsOlder, 
            bool? localMatchesRemote, bool? remoteExists, bool? localExists)
        {
            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags, 
                ResourceStatusType.LocalIsNewer, localIsNewer);

            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.LocalIsOlder, localIsOlder);

            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.LocalMatchesRemote, localMatchesRemote);

            if (remoteExists.HasValue) _resourceStatusFlags |= ResourceStatusType.RemoteExistsIsKnown;
            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.RemoteExists, remoteExists);

            if (localExists.HasValue) _resourceStatusFlags |= ResourceStatusType.LocalExistsIsKnown;
            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.LocalExists, localExists);
        }

        /// <summary>
        /// Sets the resource status flag.
        /// </summary>
        /// <param name="currentFlags">The current flags.</param>
        /// <param name="flagToToggle">The flag to toggle.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>The new resource status</returns>
        private ResourceStatusType SetResourceStatusFlag(
            ResourceStatusType currentFlags,
            ResourceStatusType flagToToggle,
            bool? newValue)
        {
            if(!newValue.HasValue) return currentFlags;

            if (newValue.Value) return currentFlags | flagToToggle;
            else return currentFlags & ~flagToToggle;
        }
    }
}
