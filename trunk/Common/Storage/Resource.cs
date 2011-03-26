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
    public sealed class Resource
    {
        private MetaAsset _metaAsset;
        private DataAsset _dataAsset;

        public MetaAsset MetaAsset { get { return _metaAsset; } }
        public DataAsset DataAsset { get { return _dataAsset; } }
        public Guid Guid { get { return _metaAsset.Guid; } }

        public Resource(Guid guid, Database cdb)
        {
            _metaAsset = new MetaAsset(guid, cdb);
            _dataAsset = new DataAsset(_metaAsset, cdb);
        }

        public bool GetMetaAssetFromRemote(Work.ResourceJobBase job, out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _metaAsset.GetFromRemote(job, out errorMessage);
        }

        public bool DownloadDataAssetAndSaveLocally(Work.ResourceJobBase job, 
            FileSystem.IO fileSystem, out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _dataAsset.DownloadAndSaveLocally(job, _metaAsset, fileSystem, out errorMessage);
        }

        public bool DownloadResourceAndSaveLocally(Work.ResourceJobBase job,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            if (!GetMetaAssetFromRemote(job, out errorMessage))
                return false;

            if (!MetaAsset.SaveToLocal(job, fileSystem))
                return false;

            if (!DownloadDataAssetAndSaveLocally(job, fileSystem, out errorMessage))
                return false;

            return true;
        }

        public bool CreateResourceOnRemote(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            out string errorMessage)
        {
            errorMessage = null;

            if (!MetaAsset.CreateOnRemote(job, out errorMessage))
                return false;

            if (!DataAsset.CreateOnRemote(job, MetaAsset, fileSystem, out errorMessage))
                return false;

            return true;
        }

        public bool UpdateMetaAssetOnRemote(Work.ResourceJobBase job, out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _metaAsset.UpdateOnRemote(job, out errorMessage);
        }

        public bool UpdateDataAssetOnRemote(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _dataAsset.UpdateOnRemote(job, _metaAsset, fileSystem, out errorMessage);
        }

        public bool UpdateResourceOnRemote(Work.ResourceJobBase job,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            if (!UpdateMetaAssetOnRemote(job, out errorMessage))
                return false;

            if (!UpdateDataAssetOnRemote(job, fileSystem, out errorMessage))
                return false;

            return true;
        }
    }
}
