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
using Common.CouchDB;

namespace Common.Storage
{
    /// <summary>
    /// A <see cref="DataAsset"/> represents a data file in a way that it is usable by the OpenDMS.NET Project.
    /// </summary>
    public sealed class DataAsset 
        : AssetBase
    {
        /// <summary>
        /// Represents the method that handles an event.
        /// </summary>
        /// <param name="sender">The <see cref="DataAsset"/> that triggered the event.</param>
        public delegate void EventHandler(DataAsset sender);
        /// <summary>
        /// Represents the method that handles a progress event.
        /// </summary>
        /// <param name="sender">The <see cref="DataAsset"/> that triggered the event.</param>
        /// <param name="percentComplete">An integer value representing the percentage of progress.</param>
        public delegate void ProgressHandler(DataAsset sender, int percentComplete);
        /// <summary>
        /// Occurs when progress is made in a long running action.
        /// </summary>
        public event ProgressHandler OnProgress;
        /// <summary>
        /// Occurs when a long running action is completed.
        /// </summary>
        public event EventHandler OnComplete;

        /// <summary>
        /// The quantity of bytes completed.
        /// </summary>
        public ulong BytesComplete;
        /// <summary>
        /// The total quantity of bytes.
        /// </summary>
        public ulong BytesTotal;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="ma">The <see cref="MetaAsset"/> that is paired with this <see cref="DataAsset"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        public DataAsset(MetaAsset ma, Database cdb)
            : this(ma.Guid, ma.Extension, cdb)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> providing a unique reference to the Asset.</param>
        /// <param name="extension">The extension of the resource (e.g., .doc, .xsl, .odt)</param>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        public DataAsset(Guid guid, string extension, Database cdb)
            : base(guid, cdb)
        {
            BytesComplete = BytesTotal = 0;
            _state = new AssetState() { State = AssetState.Flags.CanTransfer };
        }
    }
}