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

namespace OpenDMS
{
    public class Version
    {
        private Common.Logger _logger;
        private FullAsset _fullAsset;
        
        private MetaAsset _metaAsset
        {
            get { return _fullAsset.MetaAsset; }
            set { _fullAsset.MetaAsset = value; }
        }
        
        private DataAsset _dataAsset
        {
            get { return _fullAsset.DataAsset; }
            set { _fullAsset.DataAsset = value; }
        }
                
        public FullAsset FullAsset 
        {
            get { return _fullAsset; }
        }

        public MetaAsset MetaAsset
        {
            get { return _metaAsset; }
        }

        public DataAsset DataAsset
        {
            get { return _dataAsset; }
        }

        private Version(Common.Logger logger)
        {
        }

        public Version(FullAsset fullAsset, Common.Logger logger)
        {
            _logger = logger;
            _fullAsset = fullAsset;
        }

        public bool CopyMetaUsingVersionScheme()
        {
            // Copy the current version to a file
            // Meta
            if (!_metaAsset.CopyCurrentToVersionScheme())
            {
                if (_logger != null)
                    _logger.Write(Common.Logger.LevelEnum.Normal,
                        "Failed to copy the current meta asset to a destination using the version scheme.");
                return false;
            }

            return true;
        }

        public bool CopyDataUsingVersionScheme()
        {
            // Copy the current version to a file
            // Data
            if (!_dataAsset.CopyCurrentToVersionScheme(_metaAsset.DataVersion))
            {
                if (_logger != null)
                    _logger.Write(Common.Logger.LevelEnum.Normal,
                        "Failed to copy the current data asset to a destination using the version scheme.");
                return false;
            }

            return true;
        }

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
