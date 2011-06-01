using System;
using Common.Http;
using Common.Http.Methods;
using Common.Http.Network;

namespace Common.Data.Providers.CouchDB.Commands
{
    protected abstract class Base
    {
        public delegate void TimeoutDelegate(Base sender, Client client, HttpConnection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(Base sender, Client client, HttpConnection connection, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(Base sender, Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDlegate(Base sender, Client client, HttpConnection connection, HttpResponse response);
        public event CompletionDlegate OnComplete;

        protected abstract HttpRequest _httpRequest = null;

        public Uri Uri { get { return _httpRequest.Uri; } }
        public virtual HttpRequest HttpRequest { get { return _httpRequest; } }
        
        public Base(HttpRequest httpRequest)
        {
            _httpRequest = httpRequest;
        }

        public virtual void Execute()
        {
            if (_httpRequest == null)
                throw new InvalidOperationException("The HttpRequest has not been set.");

            Client client = new Client();
            client.OnComplete += new Client.CompletionDelegate(Client_OnComplete);
            client.OnError += new Client.ErrorDelegate(Client_OnError);
            client.OnProgress += new Client.ProgressDelegate(Client_OnProgress);
            client.OnTimeout += new Client.TimeoutDelegate(Client_OnTimeout);
            client.Execute(HttpRequest, 
                Globals.Network_SendTimeout, 
                Globals.Network_ReceiveTimeout, 
                Globals.Network_SendBufferSize, 
                Globals.Network_ReceiveBufferSize);
        }

        protected virtual void Client_OnTimeout(Client sender, HttpConnection connection)
        {
            if (OnTimeout != null) OnTimeout(this, sender, connection);
            else throw new UnsupportedException("OnTimeout is not supported");
        }

        protected virtual void Client_OnProgress(Client sender, HttpConnection connection, DirectionType direction, int packetSize)
        {
            if (OnProgress != null) OnProgress(this, sender, connection, direction, packetSize);
        }

        protected virtual void Client_OnError(Client sender, string message, Exception exception)
        {
            if (OnError != null) OnError(this, sender, message, exception);
            else throw new UnsupportedException("OnError is not supported");
        }

        protected virtual void Client_OnComplete(Client sender, HttpConnection connection, HttpResponse response)
        {
            if (OnComplete != null) OnComplete(this, sender, connection, response);
            else throw new UnsupportedException("OnComplete is not supported");
        }
    }
}
