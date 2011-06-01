using System;

namespace Common.Http
{
    public class Client
    {
        public delegate void ProgressDelegate(Client sender, Network.HttpConnection connection, Network.DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void TimeoutDelegate(Client sender, Network.HttpConnection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void CompletionDelegate(Client sender, Network.HttpConnection connection, Methods.HttpResponse response);
        public event CompletionDelegate OnComplete;
        public delegate void ErrorDelegate(Client sender, string message, Exception exception);
        public event ErrorDelegate OnError;

        private Methods.HttpRequest _request;
        private System.IO.Stream _stream;

        public void Execute(Methods.HttpRequest request, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            Execute(request, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
        }

        public void Execute(Methods.HttpRequest request, System.IO.Stream stream,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            Network.HttpConnectionFactory connFactory = null;
            Network.HttpConnection connection = null;

            connFactory = new Network.HttpConnectionFactory();
            connFactory.OnConnected += new Network.HttpConnectionFactory.ConnectedDelegate(ConnFactory_OnConnected);
            connFactory.OnError += new Network.HttpConnection.ErrorDelegate(ConnFactory_OnError);
            connection = connFactory.GetConnection(request.Uri, sendTimeout, receiveTimeout,
                sendBufferSize, receiveBufferSize);
        }

        private void ConnFactory_OnError(Network.HttpConnection sender, string message, Exception exception)
        {
            Logger.Network.Error("Http.Client received an error from Http.Network.HttpConnectionFactory. Message: " + message, exception);
            if (OnError != null) OnError(this, message, exception);
            else throw new ErrorNotImplementedException("No subscription to OnError.");
        }

        private void ConnFactory_OnConnected(Network.HttpConnection sender)
        {
            sender.OnProgress += new Network.HttpConnection.ProgressDelegate(Connection_OnProgress);
            sender.OnComplete += new Network.HttpConnection.CompletionEvent(Connection_OnComplete);
            sender.OnTimeout += new Network.HttpConnection.ConnectionDelegate(Connection_OnTimeout);
            sender.OnError += new Network.HttpConnection.ErrorDelegate(Connection_OnError);
            sender.OnDisconnect += new Network.HttpConnection.ConnectionDelegate(Connection_OnDisconnect);

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

        private void Connection_OnError(Network.HttpConnection sender, string message, Exception exception)
        {
            Logger.Network.Error("Http.Client received an error from Http.Network.HttpConnection. Message: " + message, exception);
            if (OnError != null) OnError(this, message, exception);
            else throw new ErrorNotImplementedException("No subscription to OnError.");
        }

        private void Connection_OnDisconnect(Network.HttpConnection sender)
        {
            // Nothing to do really.
        }

        private void Connection_OnTimeout(Network.HttpConnection sender)
        {
            Logger.Network.Error("Http.Client received an timeout from Http.Network.HttpConnection.");
            if (OnTimeout != null) OnTimeout(this, sender);
            else throw new ErrorNotImplementedException("No subscription to OnTimeout.");
        }

        private void Connection_OnComplete(Network.HttpConnection sender, Methods.HttpResponse response)
        {
            Logger.Network.Info("Http.Client received a completion event from Http.Network.HttpConnection.");
            if (OnComplete != null) OnComplete(this, sender, response);
            else throw new CompleteNotImplementedException("No subscription to OnComplete.");
        }

        private void Connection_OnProgress(Network.HttpConnection sender, Network.DirectionType direction, int packetSize)
        {
            // Not logged, way to verbose
            if (OnProgress != null) OnProgress(this, sender, direction, packetSize);
        }
    }
}
