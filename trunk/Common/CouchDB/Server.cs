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

namespace Common.CouchDB
{
    /// <summary>
    /// Represents a CouchDB server
    /// </summary>
    public class Server
    {
        /// <summary>
        /// The Default Host
        /// </summary>
        private const string DefaultHost = "localhost";
        /// <summary>
        /// The Default Host Port
        /// </summary>
        private const int DefaultPort = 5984;
        /// <summary>
        /// The Default Timeout for web transcations
        /// </summary>
        private const int DefaultTimeout = 5000; // 5 seconds
        /// <summary>
        /// The Default size of the buffer
        /// </summary>
        private const int DefaultBufferSize = 1024;

        /// <summary>
        /// The Host
        /// </summary>
        public readonly string Host;
        /// <summary>
        /// The Host Port
        /// </summary>
        public readonly int Port;
        /// <summary>
        /// The timeout for web transactions
        /// </summary>
        public readonly int Timeout;
        /// <summary>
        /// The size of the buffer
        /// </summary>
        public readonly int BufferSize;

        /// <summary>
        /// The username used to access CouchDB
        /// </summary>
        public readonly string UserName;
        /// <summary>
        /// The password used access CouchDB
        /// </summary>
        public readonly string Password;
        /// <summary>
        /// The credentials encoded
        /// </summary>
        public readonly string EncodedCredentials;

        /// <summary>
        /// A prefix that is applied before the database name
        /// </summary>
        public string DatabasePrefix = ""; // Used by databases to prefix their names

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        /// <param name="host">The server IP</param>
        /// <param name="port">The server port</param>
        /// <param name="timeout">The duration to allow connections to remain open awaiting response from the server</param>
        /// <param name="buffersize">The size of the network buffer.</param>
        /// <param name="user">The username</param>
        /// <param name="pass">The password</param>
        public Server(string host, int port, int timeout, int buffersize, string user, string pass)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host cannot be null or empty", "host");
            if (port <= 0 || port > Int16.MaxValue)
                throw new ArgumentException("Invalid port number, must be between 0 and " + Int16.MaxValue.ToString(), "port");
            if (timeout <= 0)
                throw new ArgumentException("Invalid timeout, must be greater than 0", "port");
            if (buffersize <= 0)
                throw new ArgumentException("Invalid buffersize, must be greater than 0", "port");

            Host = host;
            Port = port;
            Timeout = timeout;
            BufferSize = buffersize;
            UserName = user;
            Password = pass;

            if (!string.IsNullOrEmpty(UserName))
                EncodedCredentials = "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(UserName + ":" + Password));

            Logger.General.Debug("Common.CouchDB.Server instantiated.");
        }

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        /// <param name="host">The server IP</param>
        /// <param name="port">The server port</param>
        /// <param name="timeout">The duration to allow connections to remain open awaiting response from the server</param>
        /// <param name="buffersize">The size of the buffer.</param>
        public Server(string host, int port, int timeout, int buffersize)
            : this(host, port, timeout, buffersize, null, null)
        {
        }

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        /// <param name="host">The server IP</param>
        /// <param name="port">The server port</param>
        /// <param name="timeout">The duration to allow connections to remain open awaiting response from the server</param>
        public Server(string host, int port, int timeout)
            : this(host, port, timeout, DefaultBufferSize, null, null)
        {
        }

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        /// <param name="host">The server IP</param>
        /// <param name="port">The server port</param>
        public Server(string host, int port)
            : this(host, port, DefaultTimeout, DefaultBufferSize, null, null)
        {
        }

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        /// <param name="host">The server IP</param>
        public Server(string host)
            : this(host, DefaultPort, DefaultTimeout)
        {
        }

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        public Server()
            : this(DefaultHost, DefaultPort, DefaultTimeout)
        {
        }

        /// <summary>
        /// Gets the Server's IP and Port as a string in format "host:port"
        /// </summary>
        public string ServerName
        {
            get { return Host + ":" + Port; }
        }

        /// <summary>
        /// Gets a Couch.Database reference with the specified name
        /// </summary>
        /// <param name="name">Name of the database</param>
        /// <returns></returns>
        public Database GetDatabase(string name)
        {
            return new Database(name, this);
        }
    }
}
