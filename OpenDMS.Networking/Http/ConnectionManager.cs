using System;

namespace OpenDMS.Networking.Http
{
    public class ConnectionManager
    {
        public delegate void ConnectedDelegate(Connection sender);
        public event ConnectedDelegate OnConnected;
        public event Connection.ErrorDelegate OnError;

        public Connection GetConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            Connection conn = new Connection(this, uri, sendTimeout,
                receiveTimeout, sendBufferSize, receiveBufferSize);
            conn.IsBusy = true;
            conn.OnConnect += new Connection.ConnectionDelegate(Connection_OnConnect);
            conn.OnError += new Connection.ErrorDelegate(Connection_OnError);
            conn.ConnectAsync();
            return conn;
        }

        private void Connection_OnError(Connection sender, string message, Exception exception)
        {
            sender.OnConnect -= Connection_OnConnect;
            Logger.Network.Error("An exception occurred while attempting to create a new connection.", exception);
            if (OnError != null) OnError(sender, message, exception);
        }

        private void Connection_OnConnect(Connection sender)
        {
            sender.OnConnect -= Connection_OnConnect;
            if (OnConnected != null) OnConnected(sender);
        }
    }
}
