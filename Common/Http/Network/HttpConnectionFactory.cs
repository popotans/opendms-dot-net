using System;

namespace Common.Http.Network
{
    public class HttpConnectionFactory
    {
        public delegate void ConnectedDelegate(HttpConnection sender);
        public event ConnectedDelegate OnConnected;
        public event HttpConnection.ErrorDelegate OnError;
        
        public HttpConnection GetConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            HttpConnection conn = new HttpConnection(this, uri, sendTimeout, 
                receiveTimeout, sendBufferSize, receiveBufferSize);
            conn.IsBusy = true;
            conn.OnConnect += new HttpConnection.ConnectionDelegate(Connection_OnConnect);
            conn.OnError += new HttpConnection.ErrorDelegate(Connection_OnError);
            conn.ConnectAsync();
            return conn;
        }

        private void Connection_OnError(HttpConnection sender, string message, Exception exception)
        {
            sender.OnConnect -= Connection_OnConnect;
            Logger.Network.Error("An exception occurred while attempting to create a new connection.", exception);
            if (OnError != null) OnError(sender, message, exception);
        }

        private void Connection_OnConnect(HttpConnection sender)
        {
            sender.OnConnect -= Connection_OnConnect;
            if (OnConnected != null) OnConnected(sender);
        }
    }
}
