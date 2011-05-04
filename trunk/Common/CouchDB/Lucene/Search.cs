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

namespace Common.CouchDB.Lucene
{
    /// <summary>
    /// The Search class is used to interact with CouchDB-Lucene's search abilities
    /// </summary>
    public class Search
    {
        /// <summary>
        /// The Design Document
        /// </summary>
        private string _designDoc;

        /// <summary>
        /// The Index Name
        /// </summary>
        private string _indexName;

        /// <summary>
        /// The Server
        /// </summary>
        private Server _server;

        /// <summary>
        /// The Database
        /// </summary>
        private Database _db;

        /// <summary>
        /// The Query to run against CouchDB-Lucene
        /// </summary>
        private string _query;

        /// <summary>
        /// The results of the search
        /// </summary>
        private SearchResultCollection _searchResultCollection;

        /// <summary>
        /// Handles general events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        public delegate void EventHandler(Search sender, Http.Client httpClient);
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
        public delegate void ProgressEventHandler(Search sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);

        /// <summary>
        /// Fired when a timeout occurs - *NOTE* a timeout will prevent OnComplete from firing.
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
        /// Gets the index name
        /// </summary>
        public string IndexName
        {
            get { return _indexName; }
        }

        /// <summary>
        /// Gets the results of the search
        /// </summary>
        public SearchResultCollection SearchResultCollection
        {
            get { return _searchResultCollection; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="server">The server</param>
        /// <param name="db">The database</param>
        /// <param name="designDoc">The design document</param>
        /// <param name="indexName">The index name</param>
        public Search(Server server, Database db, string designDoc, string indexName)
        {
            if (server == null)
                throw new ArgumentException("server cannot be null");
            if (db == null)
                throw new ArgumentException("db cannot be null");
            if (string.IsNullOrEmpty(designDoc))
                throw new ArgumentException("designDoc cannot be null or empty", "designDoc");
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentException("indexName cannot be null or empty", "indexName");

            _server = server;
            _db = db;
            _designDoc = designDoc;
            _indexName = indexName;

            Logger.General.Debug("Common.CouchDB.Lucene.Search instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="server">The server</param>
        /// <param name="db">The database</param>
        /// <param name="designDoc">The design document</param>
        /// <param name="indexName">The index name</param>
        /// <param name="query">The query</param>
        public Search(Server server, Database db, string designDoc, string indexName, string query) :
            this(server, db, designDoc, indexName)
        {
            _query = query;
        }

        public Search(Server server, Database db, string designDoc, string indexName, QueryBuilder query) :
            this(server, db, designDoc, indexName, query.QueryString)
        {
        }

        #endregion

        /// <summary>
        /// Executes the search against the CouchDB-Lucene provider
        /// </summary>
        /// <typeparam name="T">Must be SearchResultCollection</typeparam>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result Get<T>(int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize) 
            where T : SearchResultCollection
        {
            string utf8 = null;
            string json;
            Http.Client httpClient;
            Http.Methods.HttpGet httpGet;
            Http.Methods.HttpResponse httpResponse = null;

            // Convert query to UTF8
            try
            {
                if (!string.IsNullOrEmpty(_query))
                    utf8 = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.ASCII.GetBytes(_query));
            }
            catch (Exception e)
            {
                throw new Exception("Failed to encode to UTF-8.", e);
            }

            // Setup
            httpClient = new Http.Client();
            httpGet = new Http.Methods.HttpGet(Utilities.BuildUriForSearch(_db, _designDoc, _indexName, utf8));

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            // Dispatch the web request
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

            if (httpResponse != null && httpResponse.ResponseCode != 200)
                return new Result(false, "Resource does not exist.");

            Logger.General.Debug("Search executed on " + _designDoc + "/" + _indexName + "?" + utf8);

            json = httpResponse.Stream.ReadToEnd();

            try
            {
                _searchResultCollection = Deserialize<SearchResultCollection>(json);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to deserialize the JSON.", e);
            }

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
        /// Deserialize (from JSON to SearchResultCollection) all results of the search
        /// </summary>
        /// <typeparam name="T">Must be SearchResultCollection</typeparam>
        /// <param name="json">The JSON to deserialize</param>
        /// <returns>SearchResultCollection of results</returns>
        private SearchResultCollection Deserialize<T>(string json) where T : SearchResultCollection
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // Register the custom converter.
            serializer.RegisterConverters(new JavaScriptConverter[] { new SearchResultCollectionConverter() });
            return serializer.Deserialize<T>(json);
        }
    }
}
