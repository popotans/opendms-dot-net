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
using Common.Data;

namespace HttpModule
{
    /// <summary>
    /// Represents a version of an Asset.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Represents a pair of <see cref="MetaAsset"/> and <see cref="DataAsset"/>
        /// </summary>
        private FullAsset _fullAsset;

        /// <summary>
        /// Gets or sets the <see cref="MetaAsset"/>.
        /// </summary>
        /// <value>
        /// The <see cref="MetaAsset"/>.
        /// </value>
        private MetaAsset _metaAsset
        {
            get { return _fullAsset.MetaAsset; }
            set { _fullAsset.MetaAsset = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataAsset"/>.
        /// </summary>
        /// <value>
        /// The <see cref="DataAsset"/>.
        /// </value>
        private DataAsset _dataAsset
        {
            get { return _fullAsset.DataAsset; }
            set { _fullAsset.DataAsset = value; }
        }

        /// <summary>
        /// Gets the <see cref="FullAsset"/>.
        /// </summary>
        public FullAsset FullAsset 
        {
            get { return _fullAsset; }
        }

        /// <summary>
        /// Gets the <see cref="MetaAsset"/>.
        /// </summary>
        public MetaAsset MetaAsset
        {
            get { return _metaAsset; }
        }

        /// <summary>
        /// Gets the <see cref="DataAsset"/>.
        /// </summary>
        public DataAsset DataAsset
        {
            get { return _dataAsset; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        private Version()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="fullAsset">Represents a pair of <see cref="MetaAsset"/> and <see cref="DataAsset"/></param>
        public Version(FullAsset fullAsset)
        {
            _fullAsset = fullAsset;
        }

        /// <summary>
        /// Copies the <see cref="MetaAsset"/> using version scheme naming.
        /// </summary>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool CopyMetaUsingVersionScheme()
        {
            // Copy the current version to a file
            // Meta
            if (!_metaAsset.CopyCurrentToVersionScheme())
            {
                Common.Logger.General.Error("Failed to copy the current meta asset to a destination using the version scheme.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copies the <see cref="DataAsset"/> using version scheme naming.
        /// </summary>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool CopyDataUsingVersionScheme()
        {
            // Copy the current version to a file
            // Data
            if (!_dataAsset.CopyCurrentToVersionScheme(_metaAsset.DataVersion))
            {
                Common.Logger.General.Error("Failed to copy the current data asset to a destination using the version scheme.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Increments the meta version.
        /// </summary>
        public void IncrementMetaVersion()
        {
            uint mv;
            ETag etag;

            mv = _metaAsset.MetaVersion + 1;
            etag = _metaAsset.ETag.Increment();

            _metaAsset.UpdateByServer(etag, mv, _metaAsset.DataVersion, _metaAsset.LockedBy, _metaAsset.LockedAt,
                _metaAsset.Creator, _metaAsset.Length, _metaAsset.Md5, _metaAsset.Created,
                _metaAsset.Modified, _metaAsset.LastAccess);
        }

        /// <summary>
        /// Increments the data version.
        /// </summary>
        public void IncrementDataVersion()
        {
            uint dv;
            ETag etag;

            dv = _metaAsset.DataVersion + 1;
            etag = _metaAsset.ETag.Increment();

            _metaAsset.UpdateByServer(etag, _metaAsset.MetaVersion, dv, _metaAsset.LockedBy, _metaAsset.LockedAt,
                _metaAsset.Creator, _metaAsset.Length, _metaAsset.Md5, _metaAsset.Created,
                _metaAsset.Modified, _metaAsset.LastAccess);
        }
    }
}
