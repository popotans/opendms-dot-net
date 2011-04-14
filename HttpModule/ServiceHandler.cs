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
            Settings.Instance = Settings.Load(@"settings\settings.xml", _fileSystem);
            if (Settings.Instance == null)
            {
                Settings.Instance = new Settings();
                Settings.Instance.Save(@"settings\settings.xml", _fileSystem);
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

            if (!_storage.GetSearchForm(userInfo["username"], out searchForm, out errorMessage))
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

            if (!_storage.GetMetaForm(userInfo["username"], out metaForm, out errorMessage))
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
        /// Responds to a checkout request applying a lock on the resource for the requesting user.
        /// http://localhost/_checkout/[guid]
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_checkout", ServicePointAttribute.VerbType.GET)]
        public void Checkout(HttpApplication app)
        {
            ServerResponse resp;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);

            resp = _storage.CheckoutResource(guid, userInfo["username"]);
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
            return;
        }

        /// <summary>
        /// Responds to a checkin request releasing the lock on the resource.
        /// http://localhost/_checkin/[guid]
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_checkin", ServicePointAttribute.VerbType.GET)]
        public void Checkin(HttpApplication app)
        {
            bool isNew = false;
            ServerResponse resp;
            Guid guid;
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            if (queryString["func"] == "new")
            {
                isNew = true;
                guid = Guid.Empty; // We can do this because Checkin, when new, assigns its own Guid
            }
            else
            {
                guid = ParseGuid(app.Request.Path);
            }

            resp = _storage.CheckinResource(guid, userInfo["username"], isNew);
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
            return;
        }

        /// <summary>
        /// Responds to an unlock request releasing the lock on the resource.
        /// http://localhost/_unlock/[guid]
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/></param>
        [ServicePoint("/_checkin", ServicePointAttribute.VerbType.GET)]
        public void Unlock(HttpApplication app)
        {
            ServerResponse resp;
            Guid guid = ParseGuid(app.Request.Path);
            Dictionary<string, string> userInfo = ParseUserInfo(app);
            Dictionary<string, string> queryString = ParseQueryString(app);

            resp = _storage.ReleaseLock(guid, userInfo["username"]);
            resp.Serialize().WriteTo(app.Response.OutputStream);
            app.CompleteRequest();
            return;
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
