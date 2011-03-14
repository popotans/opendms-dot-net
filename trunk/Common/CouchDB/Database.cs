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
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        public delegate void EventHandler(Web.WebState state, Database sender);
        /// <summary>
        /// Handles completion events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="result">The result.</param>
        public delegate void CompleteEventHandler(Web.WebState state, Database sender, Result result);

        /// <summary>
        /// Fired when a timeout occurs - *NOTE* a timeout will prevent OnComplete from firing.
        /// </summary>
        public event EventHandler OnTimeout;
        /// <summary>
        /// Fired to indicate that a web transaction is complete
        /// </summary>
        public event CompleteEventHandler OnComplete;

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
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result Create(bool keepAlive)
        {
            if (string.IsNullOrEmpty(_name))
                return new Result(false, "_name is not set", Result.INVALID_STATE);
            if (_server == null)
                return new Result(false, "_server is not set", Result.INVALID_STATE);

            Web web;
            ServerResponse sr;
            MemoryStream ms;

            // Setup
            web = new Web();
            ms = new MemoryStream();

            // Setup Event Handlers
            web.OnComplete += new Web.MessageHandler(web_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);

            Logger.General.Debug("Preparing to create database " + _name + ".");

            // Dispatch the message
            sr = web.SendMessage(_server, this, null, null, Web.OperationType.PUT, Web.DataStreamMethod.LoadToMemory, null,
                "application/json", keepAlive, false, false, false);

            Logger.General.Debug("Database " + _name + " created.");

            // Verify dispatch was successful
            return new Result(sr);
        }

        /// <summary>
        /// Asynchronously deletes an existing Database
        /// </summary>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result Delete(bool keepAlive)
        {
            if (string.IsNullOrEmpty(_name))
                return new Result(false, "_name is not set", Result.INVALID_STATE);
            if (_server == null)
                return new Result(false, "_server is not set", Result.INVALID_STATE);

            Web web;
            ServerResponse sr;
            MemoryStream ms;

            // Setup
            web = new Web();
            ms = new MemoryStream();

            // Setup Event Handlers
            web.OnComplete += new Web.MessageHandler(web_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);

            Logger.General.Debug("Preparing to delete database " + _name + ".");

            // Dispatch the message
            sr = web.SendMessage(_server, this, null, null, Web.OperationType.DELETE, Web.DataStreamMethod.LoadToMemory, null,
                "application/json", keepAlive, false, false, false);

            Logger.General.Debug("Database " + _name + " delete.");

            // Verify dispatch was successful
            return new Result(sr);
        }

        /// <summary>
        /// Called by Create or Delete if a timeout occurs to notify any consumers
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnTimeout(Web.WebState state)
        {
            if (OnTimeout != null) OnTimeout(state, this);
        }

        /// <summary>
        /// Called by Create or Delete to notify any consumers upon completion
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnComplete(Web.WebState state)
        {
            if (OnComplete != null) OnComplete(state, this, new Result(true));
        }
    }
}
