using System;

namespace Common.Http.Network
{
    public class HttpConnectionFactory
    {
        public HttpConnection GetConnection(Uri uri)
        {
            HttpConnection conn = new HttpConnection(this, uri);
            conn.IsBusy = true;
            conn.Connect();
            return conn;
        }

        public HttpConnection GetConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            HttpConnection conn = new HttpConnection(this, uri, sendTimeout, 
                receiveTimeout, sendBufferSize, receiveBufferSize);
            conn.IsBusy = true;
            conn.Connect();
            return conn;
        }

        public HttpConnection GetConnection(Uri uri, HttpConnection liveConnection)
        {
            if (liveConnection != null && liveConnection.IsConnected &&
                liveConnection.Uri.Host.ToLower() == uri.Host.ToLower())
                return liveConnection;
            else
            {
                liveConnection.Close();
                return GetConnection(uri);
            }
        }
    }
}
