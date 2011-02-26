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
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Collections.Generic;
using Common.NetworkPackage;

namespace HttpModule
{
    /// <summary>
    /// Provides a handler for HTTP requests.
    /// </summary>
    public class ServiceHandler : IDisposable
    {
        /// <summary>
        /// The <see cref="Storage.Master"/> providing a storage facility.
        /// </summary>
        private Storage.Master _storage;
        /// <summary>
        /// A reference to the <see cref="Common.FileSystem.IO"/> providing file system access.
        /// </summary>
        private Common.FileSystem.IO _fileSystem;
        /// <summary>
        /// A reference to a global set of loggers to document events.
        /// </summary>
        public static Common.Logger _logger;

        

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHandler"/> class.
        /// </summary>
        public ServiceHandler()
        {
            // File System must come first
            _fileSystem = new Common.FileSystem.IO(@"C:\DataStore\");

            // Settings should come second
            if (!_fileSystem.DirectoryExists("settings"))
                _fileSystem.CreateDirectoryPath("settings");
            Settings.Instance = Settings.Load(@"settings\Settings.xml", _fileSystem);
            if (Settings.Instance == null)
            {
                Settings.Instance = new Settings();
                Settings.Instance.SearchHostIP = "127.0.0.1";
                Settings.Instance.Save(@"settings\Settings.xml", _fileSystem);
            }

            // Third should be logging facilities
            if (!_fileSystem.DirectoryExists("logs"))
                _fileSystem.CreateDirectoryPath("logs");

            _logger = new Common.Logger(@"C:\DataStore\logs\");
 
            // Finally, storage facilities should be created
            _storage = new Storage.Master(_fileSystem);
        }


        /// <summary>
        /// Responds to a ping request.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_ping", ServicePointAttribute.VerbType.GET)]
        public void Ping(HttpApplication app)
        {
            new ServerResponse(true, ServerResponse.ErrorCode.None, "PONG").Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
        }

        /// <summary>
        /// Responds to a stats request.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_stats", ServicePointAttribute.VerbType.GET)]
        public void Stats(HttpApplication app)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Responds to a lock or releaselock request.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_lock", ServicePointAttribute.VerbType.PUT)]
        public void Lock(HttpApplication app)
        {
            ServerResponse resp;
            string errorMessage;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (queryString["releaselock"] == "true")
            {
                if (_storage.ReleaseLock(guid, userInfo["username"], out errorMessage) != Storage.ResultType.Success)
                {
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                }
                else
                {
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The resource was successfully unlocked.");
                }
            }
            else
            {
                if (_storage.ApplyLock(guid, userInfo["username"], out errorMessage) != Storage.ResultType.Success)
                {
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                }
                else
                {
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The resource was successfully locked.");
                }
            }

            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
            return;       
        }

        /// <summary>
        /// Responds to a request for the search form.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/_settings/searchform using the HTTP GET verb.</remarks>
        [ServicePoint("/_settings/searchform", ServicePointAttribute.VerbType.GET)]
        public void GetSearchForm(HttpApplication app)
        {
            SearchForm searchForm;
            ServerResponse resp;
            string errorMessage;
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("Request for search form was received from user '" + userInfo["username"] + "'.");

            if (_storage.GetSearchForm(userInfo["username"], out searchForm, out errorMessage) != Storage.ResultType.Success)
            {
                Common.Logger.General.Error("An error occurred while attempting to load the search form to respond to the request from user '" + userInfo["username"] + "'.\r\n" + errorMessage);

                resp = new ServerResponse(false, ServerResponse.ErrorCode.None, "Failed to load the search form template.");
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("Search form error message has been sent to user '" + userInfo["username"] + "'.");

                return;
            }

            searchForm.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Search form was sent to user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Responds to a request for the meta properties form.  The default form is created if it does not exist.  
        /// If it exists the system administrator can make modifications to create additional properties.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/_settings/metaform using the HTTP GET verb.</remarks>
        [ServicePoint("/_settings/metaform", ServicePointAttribute.VerbType.GET)]
        public void GetMetaForm(HttpApplication app)
        {
            MetaForm metaForm;
            string errorMessage;
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("Request for meta property form was received from user '" + userInfo["username"] + "'.");

            if (_storage.GetMetaForm(userInfo["username"], out metaForm, out errorMessage) != Storage.ResultType.Success)
            {
                Common.Logger.General.Error("The MetaPropertiesForm.xml could not be found, the default form has been saved locally to be used in the future, furthermore it was sent as a response to the request from user '" + userInfo["username"] + "'.");

                metaForm = MetaForm.GetDefault();
                metaForm.SaveToFile("settings\\metapropertiesform.xml", _fileSystem, false);
            }

            metaForm.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Meta properties form was sent to user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Responds to a search request.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/search using the HTTP GET verb.</remarks>
        [ServicePoint("/search", ServicePointAttribute.VerbType.GET)]
        public void Search(HttpApplication app)
        {
            HttpModule.Search search;
            Common.NetworkPackage.SearchResult result;
            ServerResponse response;
            Dictionary<string, string> userInfo = ParseUserInfo(app);

            Common.Logger.Network.Debug("Search request received from user '" + userInfo["username"] + "'.");

            search = new HttpModule.Search(app.Request.Url.Query, userInfo["username"], _storage);

            result = search.Execute(out response);

            if (response != null)
            {
                // Error
                response.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("Error response for search request has been sent to user '" + userInfo["username"] + "'.");

                return;
            }

            result.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for search request has been sent to user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Responds to a HEAD request for a <see cref="Common.Data.MetaAsset"/> by sending the 
        /// <see cref="Common.Data.ETag"/> version.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/meta using the HTTP HEAD verb.</remarks>
        [ServicePoint("/meta", ServicePointAttribute.VerbType.HEAD)]
        public void Head(HttpApplication app)
        {
            string errorMessage;
            ServerResponse resp;
            Common.Data.MetaAsset ma;
            Storage.ResultType result;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("Head request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            result = _storage.GetMeta(guid, userInfo["username"], true, out ma, out errorMessage);

            // If unsuccessful and not found -> kick it out
            if (result != Storage.ResultType.Success && result != Storage.ResultType.NotFound)
            {
                switch (result)
                {
                    case Storage.ResultType.IOError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.SerializationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.InstantiationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.ResourceIsLocked:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                        break;
                    default:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                }

                Common.Logger.General.Error("An " + result.ToString() + " occurred while attempting to load the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for Head request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            app.Response.AppendHeader("ETag", ma.ETag.Value);
            app.Response.AppendHeader("MD5", ma.Md5);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for Head request has been sent for id " + guid.ToString("N") + ", the ETag value was " + ma.ETag.Value.ToString() + " for user '" + userInfo["username"] + "'.");

            return;
        }

        /// <summary>
        /// Responds to a GET request for a <see cref="Common.Data.MetaAsset"/> by sending the 
        /// <see cref="Common.NetworkPackage.MetaAsset"/>.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/meta using the HTTP GET verb.</remarks>
        [ServicePoint("/meta", ServicePointAttribute.VerbType.GET)]
        public void GetMeta(HttpApplication app)
        {
            Storage.ResultType result;
            ServerResponse resp;
            string errorMessage;
            Common.Data.MetaAsset ma;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("GetMeta request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            if (queryString["readonly"] == "true")
                result = _storage.GetMeta(guid, userInfo["username"], true, out ma, out errorMessage);
            else
                result = _storage.GetMeta(guid, userInfo["username"], false, out ma, out errorMessage);

            // If unsuccessful -> kick it out
            if (result != Storage.ResultType.Success)
            {
                switch (result)
                {
                    case Storage.ResultType.NotFound:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                        break;
                    case Storage.ResultType.IOError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.SerializationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.InstantiationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.ResourceIsLocked:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                        break;
                    default:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                }

                Common.Logger.General.Error("An " + result.ToString() + " occurred while attempting to load the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for GetMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Send it to the user.
            ma.ExportToNetworkRepresentation().Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for GetMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

            return;
        }

        /// <summary>
        /// Responds to a POST request for a <see cref="Common.Data.MetaAsset"/> by receiving the stream, deserializing then saving.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/meta using the HTTP POST verb.</remarks>
        [ServicePoint("/meta", ServicePointAttribute.VerbType.POST)]
        public void CreateMeta(HttpApplication app)
        {
            Storage.ResultType result;
            Guid guid = Guid.NewGuid();
            ServerResponse resp;
            Index index = new Index();
            Common.Data.MetaAsset ma = null;
            MetaAsset netMa = new MetaAsset();
            string errorMessage;
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            // We need to get a new Guid and ensure it is unique
            while (_fileSystem.ResourceExists(Common.Data.AssetType.Meta.VirtualPath + "\\" + guid.ToString("N") + ".xml"))
            {
                guid = Guid.NewGuid();
            }

            Common.Logger.Network.Debug("CreateMeta request received to create a new resource, the id will be " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            // Deserialize the request stream
            try
            {
                netMa.Deserialize(app.Request.InputStream);

                Common.Logger.General.Error("The new meta asset that will have id " + guid.ToString("N") + " was successfully deserialized for user '" + userInfo["username"] + "'.");
            }
            catch (Exception e)
            {
                Common.Logger.General.Error("An exception occurred while attempting to deserialize the received meta asset.", e);

                resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, e.Message);
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the CreateMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Add the Guid to the netMa
            if (netMa.ContainsKey("$guid"))
                netMa["$guid"] = guid;
            else
                netMa.Add("$guid", guid);

            // Create the MetaAsset
            try
            {
                ma = Common.Data.MetaAsset.Create(netMa, _fileSystem);
            }
            catch (Exception e)
            {
                Common.Logger.General.Error("An exception occurred while attempting to instantiate a meta asset based on the received meta asset.", e);

                resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, e.Message);
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Save it
            if (queryString["releaselock"] == "true")
                result = _storage.SaveMeta(ma, userInfo["username"], true, out errorMessage);
            else
                result = _storage.SaveMeta(ma, userInfo["username"], false, out errorMessage);

            // Define the result
            switch (result)
            {
                case Storage.ResultType.NotFound:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                    break;
                case Storage.ResultType.Success:
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, guid.ToString("N"));
                    break;
                case Storage.ResultType.PermissionsError:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidPermissions, errorMessage);
                    break;
                case Storage.ResultType.ResourceIsLocked:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                    break;
                default:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                    break;
            }

            if (!(bool)resp["Pass"])
            {
                Common.Logger.General.Error("An " + result.ToString() + " occurred while attempting to save the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the CreateMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            if (!index.IndexMeta(ma, _fileSystem))
            {
                Common.Logger.General.Error("Failed to index the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.FailedIndexing,
                    "The MetaAsset could not be indexed.");
            }

            // Send the result
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for the CreateMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Responds to a PUT request for a <see cref="Common.Data.MetaAsset"/> by receiving the stream, deserializing then saving.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/meta using the HTTP PUT verb.</remarks>
        [ServicePoint("/meta", ServicePointAttribute.VerbType.PUT)]
        public void SaveMeta(HttpApplication app)
        {
            Storage.ResultType result;
            ServerResponse resp;
            Common.Data.MetaAsset ma = null;
            Index index = new Index();
            string errorMessage;
            MetaAsset netMa = new MetaAsset();
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("SaveMeta request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            try
            {
                netMa.Deserialize(app.Request.InputStream);
                Common.Logger.General.Debug("Meta asset with id " + guid.ToString("N") + " was successfully deserialized for user '" + userInfo["username"] + "'.");
            }
            catch(Exception e)
            {
                Common.Logger.General.Error("An exception occurred while attempting to deserialize the received meta asset.", e);

                resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, e.Message);
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Create the MetaAsset
            try
            {
                ma = Common.Data.MetaAsset.Create(netMa, _fileSystem);
            }
            catch (Exception e)
            {
                Common.Logger.General.Error("An exception occurred while attempting to instantiate a meta asset based on the received meta asset.", e);

                resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, e.Message);
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Make sure the Guid of the Request.Path matches the Guid in the package
            if (guid != ma.Guid)
            {
                Common.Logger.General.Error("The resource id in the url did not match the guid in the meta asset package.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidGuid,
                    "The resource id in the url did not match the guid in the package, these values must match.");
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            // Save it
            if(queryString["releaselock"] == "true")
                result = _storage.SaveMeta(ma, userInfo["username"], true, out errorMessage);
            else
                result = _storage.SaveMeta(ma, userInfo["username"], false, out errorMessage);

            // Define the result
            switch (result)
            {
                case Storage.ResultType.NotFound:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                    break;
                case Storage.ResultType.Success:
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The MetaAsset was successfully saved.");
                    break;
                case Storage.ResultType.PermissionsError:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidPermissions, errorMessage);
                    break;
                case Storage.ResultType.ResourceIsLocked:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                    break;
                default:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                    break;
            }

            if (!(bool)resp["Pass"])
            {
                Common.Logger.General.Error("An " + result.ToString() + " occurred while attempting to save the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            if (!index.IndexMeta(ma, _fileSystem))
            {
                Common.Logger.General.Error("Failed to index the meta asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.FailedIndexing,
                    "The MetaAsset could not be indexed.");
            }

            // Send the result
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for the SaveMeta request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Responds to a GET request for a <see cref="Common.Data.DataAsset"/> by sending the binary data of the data resource.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/data using the HTTP GET verb.</remarks>
        [ServicePoint("/data", ServicePointAttribute.VerbType.GET)]
        public void GetData(HttpApplication app)
        {
            Common.FileSystem.IOStream iostream;
            Storage.ResultType result;
            ServerResponse resp;
            string errorMessage;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("GetData request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            result = _storage.GetData(guid, userInfo["username"], (queryString["readonly"] == "true"), 
                out errorMessage, out iostream);
            
            // If unsuccessful -> kick it out
            if (result != Storage.ResultType.Success)
            {
                switch (result)
                {
                    case Storage.ResultType.NotFound:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                        break;
                    case Storage.ResultType.IOError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.SerializationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.InstantiationError:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                    case Storage.ResultType.ResourceIsLocked:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                        break;
                    default:
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                        break;
                }

                Common.Logger.General.Error("An " + result.ToString() + " occurred while attempting to load the data asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for GetData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            byte[] buffer = new byte[Common.SettingsBase.Instance.NetworkBufferSize];
            int bytesRead = 0;

            // Send it to the user.
            while ((bytesRead = iostream.Read(buffer, buffer.Length)) > 0)
            {
                app.Response.OutputStream.Write(buffer, 0, bytesRead);
            }

            _fileSystem.Close(iostream);

            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for GetData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

            return;
        }
        
        /// <summary>
        /// Responds to a PUT request for a <see cref="Common.Data.DataAsset"/> by copying the stream to the file system and 
        /// sends to the Solr installation for indexing.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <remarks>This is accessable at http://[host]:[port]/data using the HTTP PUT verb.</remarks>
        [ServicePoint("/data", ServicePointAttribute.VerbType.PUT)]
        public void SaveData(HttpApplication app)
        {
            // Indexing example
            // curl "http://localhost:8080/solr/update/extract?literal.id=05d51eda96a269c9d474578cb300242d&literal.extension=.odt&literal.title=Test2&literal.tags=tag2&literal.tags=tag1&commit=true" -F "myfile=@05d51eda96a269c9d474578cb300242d.odt"

            Common.Data.MetaAsset ma;
            ServerResponse resp;
            Storage.ResultType result;
            Index index = new Index();
            string errorMessage;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("SaveData request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            if (queryString["releaselock"] == "true")
                result = _storage.SaveData(guid, app.Request.InputStream, userInfo["username"], true, out ma, out errorMessage);
            else
                result = _storage.SaveData(guid, app.Request.InputStream, userInfo["username"], false, out ma, out errorMessage);

            // Define the result
            switch (result)
            {
                case Storage.ResultType.NotFound:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ResourceDoesNotExist, errorMessage);
                    break;
                case Storage.ResultType.Success:
                    resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "The DataAsset was successfully saved.");
                    break;
                case Storage.ResultType.PermissionsError:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidPermissions, errorMessage);
                    break;
                case Storage.ResultType.ResourceIsLocked:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.ReasourceIsLocked, errorMessage);
                    break;
                default:
                    resp = new ServerResponse(false, ServerResponse.ErrorCode.Exception, errorMessage);
                    break;
            }

            if (!(bool)resp["Pass"])
            {
                Common.Logger.General.Error("An " + result.ToString() + " occurred while attempting to save the data asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();

                Common.Logger.Network.Debug("An error response for the SaveData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                return;
            }

            if (!index.IndexData(guid, "data\\" + guid.ToString("N") + ma.Extension, _fileSystem))
            {
                Common.Logger.General.Error("Failed to index the data asset with id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");

                resp = new ServerResponse(false, ServerResponse.ErrorCode.FailedIndexing,
                    "The DataAsset could not be indexed.");
            }

            // Send the result
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for the SaveData request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
        }

        [ServicePoint("/tran", ServicePointAttribute.VerbType.POST)]
        public void Transaction(HttpApplication app)
        {
            Transactions.Transaction tran;
            Transactions.Coordinator tcord = new Transactions.Coordinator(_fileSystem, _storage);
            ServerResponse resp = null;
            string errMsg;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            Common.Logger.Network.Debug("Transaction manipulation request received for " + guid.ToString("N") + " by user '" + userInfo["username"] + "'.");

            if (queryString["func"] == null)
            {
                Common.Logger.Network.Info("The user '" + userInfo["username"] + "' requested a transaction manipulation but failed to indicate the desired function."); 
                resp = new ServerResponse(false, ServerResponse.ErrorCode.InvalidFunctionValue,
                    "The function (\"func\") value was not supplied.");
                resp.Serialize().WriteTo(app.Response.OutputStream);
                app.CompleteRequest();
                Common.Logger.Network.Debug("An error response for the transaction manipulation request has been sent for id " + guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
                return;
            }

            switch (queryString["func"])
            {
                case "begin":
                    tran = tcord.Begin(guid, userInfo["username"], 900000, out errMsg);
                    break;
                case "next":
                    tran = tcord.NextStep(guid, userInfo["username"], out errMsg);
                    break;
                case "undo":
                    int steps;

                    if (app.Request.QueryString["steps"] == null)
                    {
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.TransactionFailed, "The querystring argument \"steps\" is missing.");
                        Common.Logger.Network.Debug("User '" + userInfo["username"] + "' made a transaction manipulation request for undo on id " + guid.ToString("N") + ", but did not specify the \"steps\" argument.");
                    }
                    if (!int.TryParse(app.Request.QueryString["steps"], out steps))
                    {
                        resp = new ServerResponse(false, ServerResponse.ErrorCode.TransactionFailed, "Invalid value for \"steps\".");
                        Common.Logger.Network.Debug("User '" + userInfo["username"] + "' made a transaction manipulation request for undo on id " + guid.ToString("N") + ", but sent a bad value for the \"steps\" argument.");
                    }

                    if (resp != null)
                    {
                        resp.Serialize().WriteTo(app.Response.OutputStream);
                        app.CompleteRequest();
                        return;
                    }

                    tran = tcord.Undo(guid, userInfo["username"], steps, out errMsg);
                    break;
                case "abort":
                    tran = tcord.Abort(guid, userInfo["username"], out errMsg);
                    break;
                case "commit":
                    tran = tcord.Commit(guid, userInfo["username"], out errMsg);
                    break;
                default:
                    tran = null;
                    errMsg = "An unsupported function (\"func\") value was received.";
                    break;
            }

            if (tran == null)
            {
                Common.Logger.General.Info("The transaction manipulation function requested by user '" + userInfo["username"] +
                    "' on id " + guid.ToString("N") + " failed with the message: " + errMsg);
                resp = new ServerResponse(false, ServerResponse.ErrorCode.TransactionFailed, errMsg);
            }
            else
            {
                resp = new ServerResponse(true, ServerResponse.ErrorCode.None, "Transaction manipulation success.");
            }

            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();

            Common.Logger.Network.Debug("Response for the transaction manipulation request has been sent for id " + 
                guid.ToString("N") + " for user '" + userInfo["username"] + "'.");
        }

        /// <summary>
        /// Test1 creates a new resource (or a new version thereof) every time it is called
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_test1", ServicePointAttribute.VerbType.ALL)]
        public void Test1(HttpApplication app)
        {
            FileStream fs;
            string errorMessage, md5;
            Common.Data.MetaAsset ma;
            System.Collections.Generic.List<string> tags = new System.Collections.Generic.List<string>();
            Dictionary<string, object> uprop = new Dictionary<string,object>();
            tags.Add("tag1");
            tags.Add("tag2");
            tags.Add("tag3");
            uprop.Add("prop1", 1);
            uprop.Add("prop2", 1L);
            uprop.Add("prop3", 1D);
            uprop.Add("prop4", "string value");
            uprop.Add("prop5", DateTime.Now);


            md5 = _fileSystem.ComputeMd5(@"C:\ClientDataStore\Data\05d51eda96a269c9d474578cb300242d.odt");

            ma = Common.Data.MetaAsset.Create(new Guid("05d51eda96a269c9d474578cb300242d"), 
                new Common.Data.ETag("1"), 1, 1, "lucas", DateTime.Now, 
                "Lucas J. Nodine", 26703, md5, ".odt", DateTime.Now, DateTime.Now, DateTime.Now, 
                "Some document", tags, uprop, _fileSystem);
            
            if (_storage.SaveMeta(ma, "lucas", false, out errorMessage) != Storage.ResultType.Success)
            {
                app.Response.Write("Failed - " + errorMessage);
                app.CompleteRequest();
                return;
            }

            Index index = new Index();
            if(!index.IndexMeta(ma, _fileSystem))
            {
                app.Response.Write("Failed to index MetaAsset.");
                app.CompleteRequest();
                return;
            }

            fs = new FileStream(@"C:\ClientDataStore\Data\05d51eda96a269c9d474578cb300242d.odt", FileMode.Open, FileAccess.Read, FileShare.None);
            if (_storage.SaveData(ma.Guid, fs, "lucas", true, out ma, out errorMessage) != Storage.ResultType.Success)
            {
                app.Response.Write("Failed - " + errorMessage);
                app.CompleteRequest();
                return;
            }

            if (!index.IndexData(ma.Guid, "data\\" + ma.Guid.ToString("N") + ma.Extension, _fileSystem))
            {
                app.Response.Write("Failed to index DataAsset.");
                app.CompleteRequest();
                return;
            }

            app.Response.Write("Success");
            app.CompleteRequest();
        }

        /// <summary>
        /// Parses the user information from the HTTP request headers.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <returns>A collection of user information properties.</returns>
        /// <remarks>Properties contained are: username, password</remarks>
        private Dictionary<string, string> ParseUserInfo(HttpApplication app)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("username", app.Request.Headers.Get("x-lo-user"));
            dict.Add("password", app.Request.Headers.Get("x-lo-pass"));

            if (string.IsNullOrEmpty(dict["username"])) dict["username"] = "lucas";
            if (string.IsNullOrEmpty(dict["password"])) dict["password"] = "nodine";

            return dict;
        }

        /// <summary>
        /// Parses a <see cref="Guid"/> from a path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="Guid"/>.</returns>
        private Guid ParseGuid(string path)
        { 
            // http://stackoverflow.com/questions/104850/c-test-if-string-is-a-guid-without-throwing-exceptions
            // expect many less exceptions
            try
            {
                return new Guid(path.Substring(path.LastIndexOf("/")+1).Replace(".xml", ""));
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Parses keys and values representing a property from the query string.
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        /// <returns>A collection of properties where the key is a string that is the name of the property and the value is the value.</returns>
        private Dictionary<string, string> ParseQueryString(HttpApplication app)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("releaselock", null);
            dict.Add("version", null);
            dict.Add("readonly", null);
            dict.Add("func", null);

            if (!app.Request.QueryString.HasKeys())
                return dict;

            if (!string.IsNullOrEmpty(app.Request.QueryString["releaselock"]))
                dict["releaselock"] = app.Request.QueryString["releaselock"];
            if (!string.IsNullOrEmpty(app.Request.QueryString["version"]))
                dict["version"] = app.Request.QueryString["version"];
            if (!string.IsNullOrEmpty(app.Request.QueryString["readonly"]))
                dict["readonly"] = app.Request.QueryString["readonly"];
            if (!string.IsNullOrEmpty(app.Request.QueryString["func"]))
                dict["func"] = app.Request.QueryString["func"];

            return dict;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _storage = null;
            _fileSystem = null;
        }
    }
}
