using System;

namespace OpenDMS.Networking.Http
{
    public class ConnectionManager
    {
		#region Delegates and Events (3) 

		// Delegates (1) 

        public delegate void ConnectedDelegate(Connection sender);
		// Events (2) 

        public event ConnectedDelegate OnConnected;

        public event Connection.ErrorDelegate OnError;

		#endregion Delegates and Events 

		#region Methods (3) 

		// Public Methods (1) 

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
		// Private Methods (2) 

        private void Connection_OnConnect(Connection sender)
        {
            sender.OnConnect -= Connection_OnConnect;
            if (OnConnected != null) OnConnected(sender);
        }

        private void Connection_OnError(Connection sender, string message, Exception exception)
        {
            sender.OnConnect -= Connection_OnConnect;
            Logger.Network.Error("An exception occurred while attempting to create a new connection.", exception);
            if (OnError != null) OnError(sender, message, exception);
        }

		#endregion Methods 
    }
}
