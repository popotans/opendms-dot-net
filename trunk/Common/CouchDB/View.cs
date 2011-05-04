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
using System.Web.Script.Serialization;

namespace Common.CouchDB
{
    /// <summary>
    /// Represents a CouchDB View.
    /// </summary>
    public class View
    {
        /// <summary>
        /// The name of the design document
        /// </summary>
        private string _designDoc;
        /// <summary>
        /// The name of the view
        /// </summary>
        private string _viewName;
        /// <summary>
        /// The <see cref="Server"/>
        /// </summary>
        private Server _server;
        /// <summary>
        /// The <see cref="Database"/>
        /// </summary>
        private Database _db;
        /// <summary>
        /// Any query for the view
        /// </summary>
        private string _query;
        /// <summary>
        /// A <see cref="DocumentCollection"/> of results of the view
        /// </summary>
        private DocumentCollection _documentCollection;

        /// <summary>
        /// Handles general event
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        public delegate void EventHandler(View sender, Http.Client httpClient);
        /// <summary>
        /// Handles progress events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="httpClient">The <see cref="Http.Client"/> handling communications.</param>
        /// <param name="httpConnection">The <see cref="Http.HttpConnection"/> handling communications.</param>
        /// <param name="packetSize">Size of the packet.</param>
        /// <param name="headersTotal">The total size headers in bytes.</param>
        /// <param name="contentTotal">The total size of content in bytes.</param>
        /// <param name="total">The total size of all data in bytes.</param>
        public delegate void ProgressEventHandler(View sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        
        /// <summary>
        /// Occurs when a timeout event occurs.
        /// </summary>
        public event EventHandler OnTimeout;

        /// <summary>
        /// Fired to indicate progress of an upload
        /// </summary>
        public event ProgressEventHandler OnUploadProgress;

        /// <summary>
        /// Fired to indicate progress of a download
        /// </summary>
        public event ProgressEventHandler OnDownloadProgress;

        #region Properties

        /// <summary>
        /// Gets the design document
        /// </summary>
        public string DesignDoc
        {
            get { return _designDoc; }
        }
        
        /// <summary>
        /// Gets the view name
        /// </summary>
        public string ViewName
        {
            get { return _viewName; }
        }

        /// <summary>
        /// Gets the <see cref="DocumentCollection"/>
        /// </summary>
        public DocumentCollection DocumentCollection
        {
            get { return _documentCollection; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="db">The db.</param>
        /// <param name="designDoc">The design doc.</param>
        /// <param name="viewName">Name of the view.</param>
        public View(Server server, Database db, string designDoc, string viewName)
        {
            if (server == null)
                throw new ArgumentException("server cannot be null");
            if (db == null)
                throw new ArgumentException("db cannot be null");
            if (string.IsNullOrEmpty(designDoc))
                throw new ArgumentException("designDoc cannot be null or empty", "designDoc");
            if (string.IsNullOrEmpty(viewName))
                throw new ArgumentException("viewName cannot be null or empty", "viewName");

            _server = server;
            _db = db;
            _designDoc = designDoc;
            _viewName = viewName;

            Logger.General.Debug("Common.CouchDB.View instantiated.");
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="db">The db.</param>
        /// <param name="designDoc">The design doc.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="query">The query.</param>
        public View(Server server, Database db, string designDoc, string viewName, string query) :
            this(server, db, designDoc, viewName)
        {
            _query = query;
        }

        #endregion

        /// <summary>
        /// Gets the results of this instace.
        /// </summary>
        /// <typeparam name="T">Must be set to <see cref="DocumentCollection"/></typeparam>
        /// <param name="keepAlive">if set to <c>true</c> keep alive; otherwise, drop the connection when done.</param>
        /// <returns></returns>
        public Result Get<T>(int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize) 
            where T : DocumentCollection
        {
            ServerResponse sr;
            Http.Client httpClient = new Http.Client();
            Http.Methods.HttpGet httpGet = null;
            Http.Methods.HttpResponse httpResponse = null;
            string utf8 = null;

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            try
            {
                if(!string.IsNullOrEmpty(_query))
                    utf8 = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.ASCII.GetBytes(_query));
            }
            catch(Exception e)
            {
                throw new Exception("Failed to encode to UTF-8.", e);
            }

            httpGet = new Http.Methods.HttpGet(Utilities.BuildUriForView(_db, _designDoc, _viewName, utf8));

            try
            {
                httpResponse = httpClient.Execute(httpGet, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
            }
            catch (Http.Network.HttpNetworkTimeoutException e)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, httpClient);
                    return new Result(false, "Timeout");
                }
                else
                    throw e;
            }

            Logger.General.Debug("View " + _designDoc + "/" + _viewName + "?" + utf8 + " received.");

            return new Result(true);
        }

        #region Event Handling

        /// <summary>
        /// Called when a download needs to update its progress to any consumers.
        /// </summary>
        /// <param name="sender">The <see cref="Http.Client"/> that is updating.</param>
        /// <param name="connection">The <see cref="Http.Network.HttpConnection"/> handling communications.</param>
        /// <param name="packetSize">Size of the packet just received.</param>
        /// <param name="headersTotal">The size of all headers received.</param>
        /// <param name="contentTotal">The size of all content received.</param>
        /// <param name="total">The size of all data received.</param>
        void httpClient_OnDataReceived(Http.Client sender, Http.Network.HttpConnection connection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnDownloadProgress != null)
                OnDownloadProgress(this, sender, connection, packetSize, headersTotal, contentTotal, total);
        }

        /// <summary>
        /// Called when a upload needs to update its progress to any consumers.
        /// </summary>
        /// <param name="sender">The <see cref="Http.Client"/> that is updating.</param>
        /// <param name="connection">The <see cref="Http.Network.HttpConnection"/> handling communications.</param>
        /// <param name="packetSize">Size of the packet just sent.</param>
        /// <param name="headersTotal">The size of all headers sent.</param>
        /// <param name="contentTotal">The size of all content sent.</param>
        /// <param name="total">The size of all data sent.</param>
        void httpClient_OnDataSent(Http.Client sender, Http.Network.HttpConnection connection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnUploadProgress != null)
                OnUploadProgress(this, sender, connection, packetSize, headersTotal, contentTotal, total);
        }

        #endregion

        /// <summary>
        /// Deserializes the JSON into a DocumentCollection
        /// </summary>
        /// <typeparam name="T">Must be DocumentCollection</typeparam>
        /// <param name="json">JSON to deserialize</param>
        /// <returns>A DocumentCollection</returns>
        private DocumentCollection Deserialize<T>(string json) where T : DocumentCollection
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // Register the custom converter.
            serializer.RegisterConverters(new JavaScriptConverter[] { new DocumentCollectionConverter() });
            return serializer.Deserialize<T>(json);
        }
    }
}
