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

namespace Common.CouchDB
{
    /// <summary>
    /// The Database class is a representation of a database on a CouchDB server
    /// </summary>
    public class Database
    {
        /// <summary>
        /// The Database name
        /// </summary>
        private string _name;

        /// <summary>
        /// The Database Server
        /// </summary>
        private Server _server;


        /// <summary>
        /// Handle general events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="httpClient">The <see cref="Http.Client"/> handling communications.</param>
        public delegate void EventHandler(Database sender, Http.Client httpClient);
        /// <summary>
        /// Handles completion events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="result">The result.</param>
        public delegate void CompleteEventHandler(Database sender, Result result);
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
        public delegate void ProgressEventHandler(Database sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        

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

        /// <summary>
        /// Gets the Database name
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the Server object
        /// </summary>
        public Server Server
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the database</param>
        /// <param name="server">The server object</param>
        public Database(string name, Server server)
        {
            _name = name;
            _server = server;

            Logger.General.Debug("Common.CouchDB.Database instantiated.");
        }

        /// <summary>
        /// Asynchronously creates a new Database
        /// </summary>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Create(int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            if (string.IsNullOrEmpty(_name))
                return new Result(false, "_name is not set", Result.INVALID_STATE);
            if (_server == null)
                return new Result(false, "_server is not set", Result.INVALID_STATE);

            ServerResponse response;
            Http.Client httpClient;
            Http.Methods.HttpPut httpPut;
            Http.Methods.HttpResponse httpResponse = null;
            string json = null;

            // Setup
            httpClient = new Http.Client();
            httpPut = new Http.Methods.HttpPut(Utilities.BuildUri(_server), "application/json"); 

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            Logger.General.Debug("Preparing to create database " + _name + ".");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpPut, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
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

            if (httpResponse == null)
                throw new Http.Network.HttpNetworkException("The response is null.");

            Logger.General.Debug("Database " + _name + " created.");
            
            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Verify dispatch was successful
            return new Result(response);
        }

        /// <summary>
        /// Synchronously deletes an existing Database
        /// </summary>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Delete(int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            if (string.IsNullOrEmpty(_name))
                return new Result(false, "_name is not set", Result.INVALID_STATE);
            if (_server == null)
                return new Result(false, "_server is not set", Result.INVALID_STATE);

            string json;
            ServerResponse response;
            Http.Client httpClient;
            Http.Methods.HttpDelete httpDelete;
            Http.Methods.HttpResponse httpResponse = null;

            // Setup
            httpClient = new Http.Client();
            httpDelete = new Http.Methods.HttpDelete(Utilities.BuildUri(_server));

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            Logger.General.Debug("Preparing to delete database " + _name + ".");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpDelete, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
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

            if (httpResponse == null)
                throw new Http.Network.HttpNetworkException("The response is null.");

            Logger.General.Debug("Database " + _name + " deleted.");

            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Verify dispatch was successful
            return new Result(response);
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
    }
}
