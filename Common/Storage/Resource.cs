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
        private CouchDB.Database _couchdb;

        public MetaAsset MetaAsset { get { return _metaAsset; } }
        public DataAsset DataAsset { get { return _dataAsset; } }
        public Guid Guid { get { return _metaAsset.Guid; } }

        public Resource(Guid guid, Database cdb)
        {
            _couchdb = cdb;
            _metaAsset = new MetaAsset(guid, cdb);
            _dataAsset = new DataAsset(_metaAsset, cdb);
        }

        public Resource(Guid guid, string dataExtension, Database cdb)
        {
            _couchdb = cdb;
            _metaAsset = new MetaAsset(guid, cdb);
            _dataAsset = new DataAsset(guid, dataExtension, cdb);
        }

        public Resource(MetaAsset ma, Database cdb)
        {
            _couchdb = cdb;
            _metaAsset = ma;
            if (_metaAsset.Database == null) _metaAsset.Database = cdb;
            _dataAsset = new DataAsset(_metaAsset, cdb);
        }

        public bool GetMetaAssetFromRemote(Work.ResourceJobBase job, int sendBufferSize, 
            int receiveBufferSize, out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _metaAsset.GetFromRemote(job, sendBufferSize, receiveBufferSize, out errorMessage);
        }

        public bool DownloadDataAssetAndSaveLocally(Work.ResourceJobBase job, FileSystem.IO fileSystem, 
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            if (string.IsNullOrEmpty(_dataAsset.Extension))
                _dataAsset = new DataAsset(_metaAsset, _couchdb);

            return _dataAsset.DownloadAndSaveLocally(job, _metaAsset, fileSystem, sendBufferSize, 
                receiveBufferSize, out errorMessage);
        }

        public bool DownloadResourceAndSaveLocally(Work.ResourceJobBase job, FileSystem.IO fileSystem, 
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            if (!GetMetaAssetFromRemote(job, sendBufferSize, receiveBufferSize, out errorMessage))
                return false;

            if (!MetaAsset.SaveToLocal(job, fileSystem))
                return false;

            if (!DownloadDataAssetAndSaveLocally(job, fileSystem, sendBufferSize, 
                receiveBufferSize, out errorMessage))
                return false;

            return true;
        }

        public bool CreateResourceOnRemote(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            errorMessage = null;

            if (!MetaAsset.CreateOnRemote(job, sendBufferSize, receiveBufferSize, 
                out errorMessage))
                return false;

            if (!DataAsset.CreateOnRemote(job, MetaAsset, fileSystem, sendBufferSize, 
                receiveBufferSize, out errorMessage))
                return false;

            return true;
        }

        public bool UpdateMetaAssetOnRemote(Work.ResourceJobBase job, int sendBufferSize, int receiveBufferSize, 
            out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _metaAsset.UpdateOnRemote(job, sendBufferSize, receiveBufferSize, 
                out errorMessage);
        }

        public bool UpdateDataAssetOnRemote(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            errorMessage = null;

            if (_metaAsset == null)
                throw new InvalidOperationException("Meta Asset cannot be null.");

            return _dataAsset.UpdateOnRemote(job, _metaAsset, fileSystem, sendBufferSize, 
                receiveBufferSize, out errorMessage);
        }

        public bool UpdateResourceOnRemote(Work.ResourceJobBase job, FileSystem.IO fileSystem, 
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            if (!UpdateMetaAssetOnRemote(job, sendBufferSize, receiveBufferSize, out errorMessage))
                return false;

            if (!UpdateDataAssetOnRemote(job, fileSystem, sendBufferSize, receiveBufferSize, 
                out errorMessage))
                return false;

            return true;
        }

        public bool ReleaseResource(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            Http.Client httpClient;
            Http.Methods.HttpPut httpPut;
            Http.Methods.HttpResponse httpResponse;

            errorMessage = null;
            httpClient = new Http.Client();
            httpPut = new Http.Methods.HttpPut(new Uri("http://" + SettingsBase.Instance.ServerIp +
                ":" + SettingsBase.Instance.ServerPort.ToString() + "/_unlock/" +
                Guid.ToString("N")), "application/json");

            Logger.General.Debug("Sending a release resource request to the server for resource " + Guid.ToString("N"));

            job.UpdateLastAction();

            httpResponse = httpClient.Execute(httpPut, null);

            if (httpResponse.ResponseCode == 200)
            {
                Logger.General.Debug("Resource " + Guid.ToString() + " successfully released.");
                return true;
            }

            return false;
        }

        public static Resource DeepCopy(Resource resource)
        {
            return new Resource(resource.MetaAsset.Copy(resource.Guid, 
                resource.MetaAsset.Database), resource.MetaAsset.Database);
        }
    }
}
