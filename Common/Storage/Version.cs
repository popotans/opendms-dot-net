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
    public sealed class Version
    {
        public delegate void EventDelegate(Version sender);
        public delegate void ProgressDelegate(Version sender, int packetSize, ulong headersTotal
        public event VersionEvent OnTimeout;

        private MetaAsset _metaAsset;
        private DataAsset _dataAsset;
        private CouchDB.Database _couchdb;

        public MetaAsset MetaAsset { get { return _metaAsset; } }
        public DataAsset DataAsset { get { return _dataAsset; } }
        public Guid Guid { get { return _metaAsset.Guid; } }

        public Version(Guid guid, Database cdb)
        {
            _couchdb = cdb;
            _metaAsset = new MetaAsset(guid, cdb);
            _dataAsset = new DataAsset(_metaAsset, cdb);
        }

        public Version(Guid guid, string dataExtension, Database cdb)
        {
            _couchdb = cdb;
            _metaAsset = new MetaAsset(guid, cdb);
            _dataAsset = new DataAsset(guid, dataExtension, cdb);
        }

        public Version(MetaAsset ma, Database cdb)
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

            _metaAsset.OnDownloadProgress += new Storage.MetaAsset.ProgressHandler(MetaAsset_OnDownloadProgress);
            _metaAsset.OnTimeout += new Storage.MetaAsset.EventHandler(MetaAsset_OnTimeout);

            return _metaAsset.GetFromRemote(job, sendBufferSize, receiveBufferSize, out errorMessage);
        }

        void MetaAsset_OnTimeout(MetaAsset sender)
        {
            throw new NotImplementedException();
        }

        void MetaAsset_OnDownloadProgress(MetaAsset sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            throw new NotImplementedException();
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

        public bool CheckoutResource(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            NetworkPackage.ServerResponse response;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Http.Client httpClient;
            Http.Methods.HttpGet httpGet;
            Http.Methods.HttpResponse httpResponse;

            errorMessage = null;
            httpClient = new Http.Client();
            httpGet = new Http.Methods.HttpGet(new Uri("http://" + SettingsBase.Instance.ServerIp +
                ":" + SettingsBase.Instance.ServerPort.ToString() + "/_checkoutV/" +
                Guid.ToString("N")));

            Logger.General.Debug("Sending a checkout resource request to the server for resource " + Guid.ToString("N"));

            job.UpdateLastAction();

            httpResponse = httpClient.Execute(httpGet, null, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);

            httpResponse.Stream.CopyTo(ms);

            // Test code
            ms.Position = 0;
            string str = Utilities.StreamToUtf8String(ms);

            response = new NetworkPackage.ServerResponse();
            response.Deserialize(ms);

            if ((bool)response["Pass"])
            {
                Logger.General.Debug("Resource " + Guid.ToString() + " successfully checked-out.");
                return true;
            }

            errorMessage = response["Message"].ToString();
            return false;
        }

        public bool ReleaseResource(Work.ResourceJobBase job, FileSystem.IO fileSystem,
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            Http.Client httpClient;
            Http.Methods.HttpDelete httpDelete;
            Http.Methods.HttpResponse httpResponse;

            errorMessage = null;
            httpClient = new Http.Client();
            httpDelete = new Http.Methods.HttpDelete(new Uri("http://" + SettingsBase.Instance.ServerIp +
                ":" + SettingsBase.Instance.ServerPort.ToString() + "/_delete/" +
                Guid.ToString("N")));

            Logger.General.Debug("Sending a release resource request to the server for resource " + Guid.ToString("N"));

            job.UpdateLastAction();

            httpResponse = httpClient.Execute(httpDelete, null, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);

            if (httpResponse.ResponseCode == 200)
            {
                Logger.General.Debug("Resource " + Guid.ToString() + " successfully released.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the resource from remote host (should be used by a client).
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool DeleteResourceFromRemote(Work.ResourceJobBase job,
            int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            Http.Client httpClient;
            Http.Methods.HttpDelete httpDelete;
            Http.Methods.HttpResponse httpResponse;

            errorMessage = null;
            httpClient = new Http.Client();
            httpDelete = new Http.Methods.HttpDelete(new Uri("http://" + SettingsBase.Instance.ServerIp +
                ":" + SettingsBase.Instance.ServerPort.ToString() + "/_delete/" +
                Guid.ToString("N")));

            Logger.General.Debug("Sending a delete resource request to the server for resource " + Guid.ToString("N"));

            job.UpdateLastAction();

            httpResponse = httpClient.Execute(httpDelete, null, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);

            if (httpResponse.ResponseCode == 200)
            {
                Logger.General.Debug("Resource " + Guid.ToString() + " successfully deleted.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the resource from the couch DB server ONLY (should be used by the HttpModule).
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="resourceDeleted">The resource deleted.</param>
        /// <param name="versionsDeleted">The versions deleted.</param>
        /// <returns></returns>
        public bool DeleteResourceFromCouchDB(Work.RecreateResourceJob job,
            int sendBufferSize, int receiveBufferSize, out string errorMessage,
            out Common.Postgres.Resource resourceDeleted,
            out System.Collections.Generic.List<Common.Postgres.Version> versionsDeleted)
        {
            int retry = 2;
            Result result;
            CouchDB.Document doc;
            System.Collections.Generic.Dictionary<string, string> idRevCollection =
                new System.Collections.Generic.Dictionary<string, string>();

            resourceDeleted = Postgres.Resource.GetResourceFromVersionId(job.InputResource.Guid);
            versionsDeleted = resourceDeleted.GetAllVersions();

            // Get a collection of id/rev for deletion
            for (int i = 0; i < versionsDeleted.Count; i++)
            {
                doc = new Document(versionsDeleted[i].VersionGuid.ToString("N"));
                result = doc.Download(_couchdb, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);
                while (!result.IsPass & retry > 0)
                {
                    retry--;
                    result = doc.Download(_couchdb, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);
                }
                if (!result.IsPass)
                {
                    errorMessage = "Failed to download the resource information.";
                    return false;
                }
                idRevCollection.Add(doc.Id, doc.Rev);
            }

            System.Collections.Generic.Dictionary<string, string>.Enumerator en =
                idRevCollection.GetEnumerator();

            // Delete
            while(en.MoveNext())
            {
                doc = new Document(en.Current.Key, en.Current.Value);
                result = doc.Delete(_couchdb, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);
                while (!result.IsPass & retry > 0)
                {
                    retry--;
                    result = doc.Download(_couchdb, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize, job);
                }
                if (!result.IsPass)
                {
                    errorMessage = "Failed to delete the resource.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        public static Version DeepCopy(Version resource)
        {
            return new Version(resource.MetaAsset.Copy(resource.Guid, 
                resource.MetaAsset.Database), resource.MetaAsset.Database);
        }
    }
}
