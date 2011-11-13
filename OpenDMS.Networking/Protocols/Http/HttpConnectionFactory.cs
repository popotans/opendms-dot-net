using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpConnectionFactory
    {
        private Dictionary<Uri, HttpConnection> _httpConnections;

        public delegate void ConnectionDelegate(HttpConnection sender);

        public event ConnectionDelegate OnConnected;
        public event ConnectionDelegate OnError;

        public HttpConnectionFactory()
        {
            _httpConnections = new Dictionary<Uri, HttpConnection>();
        }

        public HttpConnection GetConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            HttpConnection conn;

            lock (_httpConnections)
            {
                if (_httpConnections.ContainsKey(uri))
                {
                    conn = _httpConnections[uri];
                }
                else
                {
                    conn = new HttpConnection(uri, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
                    _httpConnections.Add(uri, conn);
                }
            }

            return conn;
        }

        public void ReleaseConnection(HttpConnection connection)
        {
            lock (_httpConnections)
            {
                if (_httpConnections.ContainsKey(connection.Uri))
                {
                    _httpConnections.Remove(connection.Uri);
                }
            }
        }
    }
}
