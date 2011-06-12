using System;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;


namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public abstract class Base
    {
        public delegate void TimeoutDelegate(Base sender, Client client, Connection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(Base sender, Client client, Connection connection, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(Base sender, Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDlegate(Base sender, Client client, Connection connection, ReplyBase reply);
        public event CompletionDlegate OnComplete;

        protected Request _httpRequest;

        public Uri Uri { get { return _httpRequest.Uri; } }
        public virtual Request HttpRequest { get { return _httpRequest; } }

        public Base(Request httpRequest)
        {
            _httpRequest = httpRequest;
        }

        public virtual void Execute(int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            if (_httpRequest == null)
                throw new InvalidOperationException("The HttpRequest has not been set.");

            Client client = new Client();
            client.OnComplete += new Client.CompletionDelegate(Client_OnComplete);
            client.OnError += new Client.ErrorDelegate(Client_OnError);
            client.OnProgress += new Client.ProgressDelegate(Client_OnProgress);
            client.OnTimeout += new Client.TimeoutDelegate(Client_OnTimeout);
            client.Execute(HttpRequest,
                sendTimeout,
                receiveTimeout,
                sendBufferSize,
                receiveBufferSize);
        }

        protected virtual void Client_OnTimeout(Client sender, Connection connection)
        {
            if (OnTimeout != null) OnTimeout(this, sender, connection);
            else throw new UnsupportedException("OnTimeout is not supported");
        }

        protected virtual void Client_OnProgress(Client sender, Connection connection, DirectionType direction, int packetSize)
        {
            if (OnProgress != null) OnProgress(this, sender, connection, direction, packetSize);
        }

        protected virtual void Client_OnError(Client sender, string message, Exception exception)
        {
            if (OnError != null) OnError(this, sender, message, exception);
            else throw new UnsupportedException("OnError is not supported");
        }

        protected virtual void Client_OnComplete(Client sender, Connection connection, Response response)
        {
            if (OnComplete != null) OnComplete(this, sender, connection, MakeReply(response));
            else throw new UnsupportedException("OnComplete is not supported");
        }

        public abstract ReplyBase MakeReply(Response response);
    }
}
