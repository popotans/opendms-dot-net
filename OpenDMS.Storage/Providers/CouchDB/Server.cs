using System;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Server : IServer
    {
        private string _protocol = "http";
        private string _host = "localhost";
        private int _port = 5984;
        private int _timeout = 5000;
        private int _bufferSize = 8192;

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        public string Protocol { get { return _protocol; } }
        /// <summary>
        /// The Host
        /// </summary>
        public string Host { get { return _host; } }
        /// <summary>
        /// The Host Port
        /// </summary>
        public int Port { get { return _port; } }
        /// <summary>
        /// The timeout for web transactions
        /// </summary>
        public int Timeout { get { return _timeout; } }
        /// <summary>
        /// The size of the buffer
        /// </summary>
        public int BufferSize { get { return _bufferSize; } }

        /// <summary>
        /// Gets the URI.
        /// </summary>
        public Uri Uri { get { return new Uri(string.Format("{0}://{1}:{2}/", _protocol, _host, _port.ToString())); } }

        /// <summary>
        /// Constructor - Assigns values from arguments
        /// </summary>
        /// <param name="host">The server IP</param>
        /// <param name="port">The server port</param>
        /// <param name="timeout">The duration to allow connections to remain open awaiting response from the server</param>
        /// <param name="buffersize">The size of the network buffer.</param>
        public Server(string protocol, string host, int port, int timeout, int buffersize)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host cannot be null or empty", "host");
            if (port <= 0 || port > Int16.MaxValue)
                throw new ArgumentException("Invalid port number, must be between 0 and " + Int16.MaxValue.ToString(), "port");
            if (timeout <= 0)
                throw new ArgumentException("Invalid timeout, must be greater than 0", "port");
            if (buffersize <= 0)
                throw new ArgumentException("Invalid buffersize, must be greater than 0", "port");

            _host = host;
            _port = port;
            _timeout = timeout;
            _bufferSize = buffersize;

            //if (!string.IsNullOrEmpty(UserName))
            //    EncodedCredentials = "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(UserName + ":" + Password));
        }

        /// <summary>
        /// Gets a Couch.Database reference with the specified name
        /// </summary>
        /// <param name="name">Name of the database</param>
        /// <returns></returns>
        IDatabase IServer.GetDatabase(string name, Security.DatabaseSessionManager sessionManager)
        {
            return new Database(this, name, sessionManager);
        }
    }
}
