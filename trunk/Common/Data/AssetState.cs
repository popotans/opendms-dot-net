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

namespace Common.Data
{
    /// <summary>
    /// Represents the state of an Asset.  The state should be checked before any action on or by the Asset.
    /// </summary>
    public class AssetState
    {
        /// <summary>
        /// A collection of markers identifying the current state
        /// </summary>
        public enum Flags
        {
            /// <summary>
            /// No flag set.
            /// </summary>
            None = 0,
            /// <summary>
            /// The Asset was loaded from the local file system.
            /// </summary>
            LoadedFromLocal = 1,
            /// <summary>
            /// The Asset was loaded from the remote host.
            /// </summary>
            LoadedFromRemote = 2,
            /// <summary>
            /// The Asset is currently present in memory.
            /// </summary>
            InMemory = 4,
            /// <summary>
            /// The Asset in memory is outdated.
            /// </summary>
            MemoryDirty = 8,
            /// <summary>
            /// The Asset exists on the local file system.
            /// </summary>
            OnDisk = 16,
            /// <summary>
            /// The Asset on the local file system is outdated.
            /// </summary>
            DiskDirty = 32,
            /// <summary>
            /// The Asset can be transfered to a remote host.
            /// </summary>
            CanTransfer
        }

        /// <summary>
        /// Gets or sets a collection of markers identifying the current state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public Flags State { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetState"/> class.
        /// </summary>
        public AssetState()
        {
            this.State = AssetState.Flags.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetState"/> class.
        /// </summary>
        /// <param name="flags">A collection of markers identifying the current state.</param>
        public AssetState(Flags flags)
        {
            this.State = flags;
        }

        /// <summary>
        /// Determines whether this <see cref="AssetState"/> has the specified flag(s).
        /// </summary>
        /// <param name="flags">The <see cref="Flags"/> to check for existance.</param>
        /// <returns>
        ///   <c>true</c> if the specified flags has flag; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This method provides a shortcut to a bitwise and comparison and therefore, 
        /// multiple flags can be passed as an argument but all argument flags must match else 
        /// <c>false</c> is returned.</remarks>
        /// <example>
        /// This sample shows how to call the <see cref="HasFlag"/> method.
        /// <code>
        /// // This code assumes the programmer wants to save an asset '_asset' which has 
        /// // already been instantiated, to disk if it does not already exist on disk.
        /// void A()
        /// {
        ///     if (!_asset.State.HasFlag(AssetState.Flags.OnDisk))
        ///         _asset.Save();
        /// }
        /// </code>
        /// </example>
        public bool HasFlag(Flags flags)
        {
            return (State & flags) == flags;
        }
    }
}
