using System;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;


namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public abstract class Base
    {
        public delegate void TimeoutDelegate(Base sender, Client client, Connection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(Base sender, Client client, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(Base sender, Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDelegate(Base sender, Client client, Connection connection, ReplyBase reply);
        public event CompletionDelegate OnComplete;
        public delegate void CloseDelegate(Base sender, Client client, Connection connection);
        public event CloseDelegate OnClose;

        protected Request _httpRequest;
        protected System.IO.Stream _stream;
        protected Client _client;

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

            _client = new Client();
            _client.OnComplete += new Client.CompletionDelegate(Client_OnComplete);
            _client.OnError += new Client.ErrorDelegate(Client_OnError);
            _client.OnProgress += new Client.ProgressDelegate(Client_OnProgress);
            _client.OnTimeout += new Client.TimeoutDelegate(Client_OnTimeout);
            _client.OnClose += new Client.CloseDelegate(Client_OnClose);

            if (_stream == null)
                _client.Execute(HttpRequest,
                    sendTimeout,
                    receiveTimeout,
                    sendBufferSize,
                    receiveBufferSize);
            else
                _client.Execute(HttpRequest,
                    _stream,
                    sendTimeout,
                    receiveTimeout,
                    sendBufferSize,
                    receiveBufferSize);
        }

        public virtual void Close()
        {
            _client.Close();
        }

        protected virtual void Client_OnClose(Client sender, Connection connection)
        {
            if (OnClose != null) OnClose(this, sender, connection);
        }

        protected virtual void Client_OnTimeout(Client sender, Connection connection)
        {
            if (OnTimeout != null) OnTimeout(this, sender, connection);
            else throw new UnsupportedException("OnTimeout is not supported");
        }

        protected virtual void Client_OnProgress(Client sender, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (OnProgress != null) OnProgress(this, sender, connection, direction, packetSize, sendPercentComplete, receivePercentComplete);
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
