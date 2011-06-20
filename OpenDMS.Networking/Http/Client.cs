using System;

namespace OpenDMS.Networking.Http
{
    public class Client
    {
        public delegate void ProgressDelegate(Client sender, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public event ProgressDelegate OnProgress;
        public delegate void TimeoutDelegate(Client sender, Connection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void CompletionDelegate(Client sender, Connection connection, Methods.Response response);
        public event CompletionDelegate OnComplete;
        public delegate void ErrorDelegate(Client sender, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CloseDelegate(Client sender, Connection connection);
        public event CloseDelegate OnClose;

        private Methods.Request _request;
        private System.IO.Stream _stream;
        private Connection _connection;

        public void Execute(Methods.Request request, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            Execute(request, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
        }

        public void Execute(Methods.Request request, System.IO.Stream stream,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            ConnectionManager connMgr = null;

            connMgr = new ConnectionManager();

            _request = request;
            _stream = stream;

            connMgr.OnConnected += new ConnectionManager.ConnectedDelegate(ConnectionManager_OnConnected);
            connMgr.OnError += new Connection.ErrorDelegate(ConnectionManager_OnError);
            _connection = connMgr.GetConnection(request.Uri, sendTimeout, receiveTimeout,
                sendBufferSize, receiveBufferSize);
        }

        public void Close()
        {
            if (_connection != null && _connection.IsConnected)
                _connection.CloseAsync();
        }

        private void ConnectionManager_OnError(Connection sender, string message, Exception exception)
        {
            Logger.Network.Error("Http.Client received an error from Http.Network.HttpConnectionFactory. Message: " + message, exception);
            if (OnError != null) OnError(this, message, exception);
            else throw new ErrorNotImplementedException("No subscription to OnError.");
        }

        private void ConnectionManager_OnConnected(Connection sender)
        {
            sender.OnProgress += new Connection.ProgressDelegate(Connection_OnProgress);
            sender.OnComplete += new Connection.CompletionEvent(Connection_OnComplete);
            sender.OnTimeout += new Connection.ConnectionDelegate(Connection_OnTimeout);
            sender.OnError += new Connection.ErrorDelegate(Connection_OnError);
            sender.OnDisconnect += new Connection.ConnectionDelegate(Connection_OnDisconnect);

            try
            {
                sender.SendRequest(_request, _stream);
            }
            catch (Exception e)
            {
                sender.OnProgress -= Connection_OnProgress;
                sender.OnComplete -= Connection_OnComplete;
                sender.OnTimeout -= Connection_OnTimeout;
                sender.OnError -= Connection_OnError;
                sender.OnDisconnect -= Connection_OnDisconnect;

                Logger.Network.Error("An exception occurred while sending the request.", e);
                if (OnError != null) OnError(this, "Exception while sending the request.", e);
                else throw;
            }
        }

        private void Connection_OnError(Connection sender, string message, Exception exception)
        {
            Logger.Network.Error("Http.Client received an error from Http.Network.HttpConnection. Message: " + message, exception);
            if (OnError != null) OnError(this, message, exception);
            else throw new ErrorNotImplementedException("No subscription to OnError.");
        }

        private void Connection_OnDisconnect(Connection sender)
        {
            Logger.Network.Debug("Http.Client received a close event from Http.Network.HttpConnection.");
            if (OnClose != null) OnClose(this, sender);
        }

        private void Connection_OnTimeout(Connection sender)
        {
            Logger.Network.Error("Http.Client received an timeout from Http.Network.HttpConnection.");
            if (OnTimeout != null) OnTimeout(this, sender);
            else throw new ErrorNotImplementedException("No subscription to OnTimeout.");
        }

        private void Connection_OnComplete(Connection sender, Methods.Response response)
        {
            Logger.Network.Info("Http.Client received a completion event from Http.Network.HttpConnection.");
            if (OnComplete != null) OnComplete(this, sender, response);
            else throw new CompleteNotImplementedException("No subscription to OnComplete.");
        }

        private void Connection_OnProgress(Connection sender, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            // Not logged, way to verbose
            if (OnProgress != null) OnProgress(this, sender, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }
    }
}
