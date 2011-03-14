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
        public delegate void EventHandler(Web.WebState state, View sender);
        /// <summary>
        /// Handles completion events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="result">The result.</param>
        public delegate void CompleteEventHandler(Web.WebState state, View sender, Result result);

        /// <summary>
        /// Occurs before headers are locked.
        /// </summary>
        public event EventHandler OnBeforeHeadersAreLocked;
        /// <summary>
        /// Occurs before BeginGetRequestStream is called.
        /// </summary>
        public event EventHandler OnBeforeBeginGetRequestStream;
        /// <summary>
        /// Occurs after EndGetRequestStream is called.
        /// </summary>
        public event EventHandler OnAfterEndGetRequestStream;
        /// <summary>
        /// Occurs before BeginGetResponse is called.
        /// </summary>
        public event EventHandler OnBeforeBeginGetResponse;
        /// <summary>
        /// Occurs after EndGetResponse is called.
        /// </summary>
        public event EventHandler OnAfterEndGetResponse;
        /// <summary>
        /// Occurs when a timeout event occurs.
        /// </summary>
        public event EventHandler OnTimeout;
        /// <summary>
        /// Occurs when a completion event occurs.
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
        public Result Get<T>(bool keepAlive) where T : DocumentCollection
        {
            ServerResponse sr;
            Web web = new Web();
            string utf8 = null;

            web.OnAfterEndGetRequestStream += new Web.MessageHandler(web_OnAfterEndGetRequestStream);
            web.OnAfterEndGetResponse += new Web.MessageHandler(web_OnAfterEndGetResponse);
            web.OnBeforeBeginGetRequestStream += new Web.MessageHandler(web_OnBeforeBeginGetRequestStream);
            web.OnBeforeBeginGetResponse += new Web.MessageHandler(web_OnBeforeBeginGetResponse);
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnComplete += new Web.MessageHandler(web_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);

            try
            {
                if(!string.IsNullOrEmpty(_query))
                    utf8 = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.ASCII.GetBytes(_query));
            }
            catch(Exception e)
            {
                throw new Exception("Failed to encode to UTF-8.", e);
            }

            try
            {
                sr = web.SendMessage(_server, _db, "_design/" + _designDoc + "/_view/" + _viewName, utf8, Web.OperationType.GET,
                    Web.DataStreamMethod.LoadToMemory, null, "application/json", keepAlive, false, false, false);
            }
            catch(Exception e)
            {
                return new Result(false, "Unable to get the view results", e);
            }

            Logger.General.Debug("View " + _designDoc + "/" + _viewName + "?" + utf8 + " received.");

            return new Result(sr);
        }


        #region Event Handling

        /// <summary>
        /// Called when a timeout occurrs
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnTimeout(Web.WebState state)
        {
            if (OnTimeout != null) OnTimeout(state, this);
        }

        /// <summary>
        /// Called when the results are received
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnComplete(Web.WebState state)
        {
            StreamReader sr;
            string json;

            sr = new StreamReader(state.Stream);
            json = sr.ReadToEnd();

            try
            {
                _documentCollection = Deserialize<DocumentCollection>(json);
            }
            catch(Exception e)
            {
                throw new Exception("Failed to deserialize the JSON.", e);
            }

            if (OnComplete != null) OnComplete(state, this, new Result(true));
        }

        /// <summary>
        /// Calls OnBeforeHeadersAreLocked
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnBeforeHeadersAreLocked(Web.WebState state)
        {
            if (OnBeforeHeadersAreLocked != null) OnBeforeHeadersAreLocked(state, this);
        }

        /// <summary>
        /// Calls OnBeforeBeginGetResponse
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnBeforeBeginGetResponse(Web.WebState state)
        {
            if (OnBeforeBeginGetResponse != null) OnBeforeBeginGetResponse(state, this);
        }

        /// <summary>
        /// Calls OnBeforeBeginGetRequestStream
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnBeforeBeginGetRequestStream(Web.WebState state)
        {
            if (OnBeforeBeginGetRequestStream != null) OnBeforeBeginGetRequestStream(state, this);
        }

        /// <summary>
        /// Calls OnAfterEndGetResponse
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnAfterEndGetResponse(Web.WebState state)
        {
            if (OnAfterEndGetResponse != null) OnAfterEndGetResponse(state, this);
        }

        /// <summary>
        /// Calls OnAfterEndGetRequestStream
        /// </summary>
        /// <param name="state">The state.</param>
        void web_OnAfterEndGetRequestStream(Web.WebState state)
        {
            if (OnAfterEndGetRequestStream != null) OnAfterEndGetRequestStream(state, this);
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
