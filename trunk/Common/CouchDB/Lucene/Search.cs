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
        public delegate void EventHandler(Web.WebState state, Search sender);
        /// <summary>
        /// Handles completion events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="result">The result.</param>
        public delegate void CompleteEventHandler(Web.WebState state, Search sender, Result result);

        /// <summary>
        /// Fired before HTTP headers are locked
        /// </summary>
        public event EventHandler OnBeforeHeadersAreLocked;
        /// <summary>
        /// Fired before the request stream is read and sent
        /// </summary>
        public event EventHandler OnBeforeBeginGetRequestStream;
        /// <summary>
        /// Fired after reading and sending of the request stream is finished
        /// </summary>
        public event EventHandler OnAfterEndGetRequestStream;
        /// <summary>
        /// Fired before attempting to receive the server's response
        /// </summary>
        public event EventHandler OnBeforeBeginGetResponse;
        /// <summary>
        /// Fired after receiving the server's response
        /// </summary>
        public event EventHandler OnAfterEndGetResponse;
        /// <summary>
        /// Fired when a timeout occurs - *NOTE* a timeout will prevent OnComplete from firing.
        /// </summary>
        public event EventHandler OnTimeout;

        /// <summary>
        /// Fired to indicate that a web transaction is complete
        /// </summary>
        public event CompleteEventHandler OnComplete;

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
        public Result Get<T>(bool keepAlive) where T : SearchResultCollection
        {
            ServerResponse sr;

            // Setup
            Web web = new Web();
            string utf8 = null;

            // Setup Event Handlers
            web.OnAfterEndGetRequestStream += new Web.MessageHandler(web_OnAfterEndGetRequestStream);
            web.OnAfterEndGetResponse += new Web.MessageHandler(web_OnAfterEndGetResponse);
            web.OnBeforeBeginGetRequestStream += new Web.MessageHandler(web_OnBeforeBeginGetRequestStream);
            web.OnBeforeBeginGetResponse += new Web.MessageHandler(web_OnBeforeBeginGetResponse);
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnComplete += new Web.MessageHandler(web_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);

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

            // Dispatch the web request
            try
            {
                sr = web.SendMessage(_server, _db, "_fti/_design/" + _designDoc + "/" + _indexName, utf8, Web.OperationType.GET,
                    Web.DataStreamMethod.LoadToMemory, null, "application/json", keepAlive, false, false, false);
            }
            catch (Exception e)
            {
                return new Result(false, "Unable to get the search results", e);
            }

            Logger.General.Debug("Search executed on " + _designDoc + "/" + _indexName + "?" + utf8);

            return new Result(sr);
        }


        #region Event Handling

        /// <summary>
        /// Called to notify any consumers of a timeout
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnTimeout(Web.WebState state)
        {
            if (OnTimeout != null) OnTimeout(state, this);
        }

        /// <summary>
        /// Called when the web transaction is complete to notify consumers
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnComplete(Web.WebState state)
        {
            StreamReader sr;
            string json;

            // Read and close
            sr = new StreamReader(state.Stream);
            json = sr.ReadToEnd();
            sr.Close();

            try
            {
                _searchResultCollection = Deserialize<SearchResultCollection>(json);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to deserialize the JSON.", e);
            }

            if (OnComplete != null) OnComplete(state, this, new Result(true));
        }

        /// <summary>
        /// Called by any network method to notify consumers that the HTTP headers are about to be locked
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnBeforeHeadersAreLocked(Web.WebState state)
        {
            if (OnBeforeHeadersAreLocked != null) OnBeforeHeadersAreLocked(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that receiving of the server's response is about to begin
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnBeforeBeginGetResponse(Web.WebState state)
        {
            if (OnBeforeBeginGetResponse != null) OnBeforeBeginGetResponse(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that the request stream is about to be read and sent
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnBeforeBeginGetRequestStream(Web.WebState state)
        {
            if (OnBeforeBeginGetRequestStream != null) OnBeforeBeginGetRequestStream(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that the server's response has been received
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnAfterEndGetResponse(Web.WebState state)
        {
            if (OnAfterEndGetResponse != null) OnAfterEndGetResponse(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that reading and sending of the request stream are finished
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnAfterEndGetRequestStream(Web.WebState state)
        {
            if (OnAfterEndGetRequestStream != null) OnAfterEndGetRequestStream(state, this);
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
