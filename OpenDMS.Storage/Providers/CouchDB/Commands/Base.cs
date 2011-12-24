using System;
using Http = OpenDMS.Networking.Protocols.Http;
using Tcp = OpenDMS.Networking.Protocols.Tcp;


namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public abstract class Base
    {
        public delegate void TimeoutDelegate(Base sender, Http.Client client, Http.HttpConnection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(Base sender, Http.Client client, Http.HttpConnection connection, Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(Base sender, Http.Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDelegate(Base sender, Http.Client client, Http.HttpConnection connection, ReplyBase reply);
        public event CompletionDelegate OnComplete;
        public delegate void CloseDelegate(Base sender, Http.Client client, Http.HttpConnection connection);
        public event CloseDelegate OnClose;

        protected Http.Request _httpRequest;
        protected System.IO.Stream _stream;
        protected Http.Client _client;

        public Uri Uri { get; private set; }
        public virtual Http.Request HttpRequest { get { return _httpRequest; } }

        public Base(Uri uri, Http.Request httpRequest)
        {
            _httpRequest = httpRequest;
            Uri = uri;
        }

        public Base(Uri uri, Http.Methods.Base method)
            : this(uri, new Http.Request(method, uri))
        {
            _httpRequest.ContentLength = 0;
        }

        public virtual void Execute(int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            if (_httpRequest == null)
                throw new InvalidOperationException("The HttpRequest has not been set.");

            Http.HttpConnection.ConnectionDelegate onDisconnect = null, onTimeout = null, onConnect = null;
            Http.HttpConnection.CompletionDelegate onComplete = null;
            Http.HttpConnection.ErrorDelegate onError = null;
            Http.HttpConnection.ProgressDelegate onProgress = null;

            _client = new Http.Client();

            onConnect += delegate(Http.HttpConnection sender)
            {
                Logger.Storage.Debug("Client connected.");
            };
            onDisconnect += delegate(Http.HttpConnection sender)
            {
                Logger.Storage.Debug("Client closed.");
                if (OnClose != null) OnClose(this, _client, sender);
            };
            onTimeout += delegate(Http.HttpConnection sender)
            {
                Logger.Storage.Error("The command timed out.");
                if (OnTimeout != null) OnTimeout(this, _client, sender);
                else throw new UnsupportedException("OnTimeout is not supported");
            };
            onComplete += delegate(Http.HttpConnection sender, Http.Response response)
            {
                Logger.Storage.Debug("The command completed.");
                ReplyBase reply = null;

                try
                {
                    reply = MakeReply(response);
                }
                catch (Exception e)
                {
                    Logger.Storage.Error("An exception occurred while attempting to create the reply object.", e);
                    OnError(this, _client, "An exception occurred while attempting to create the reply object.", e);
                }

                if (reply != null)
                {
                    if (OnComplete != null) OnComplete(this, _client, sender, reply);
                    else throw new UnsupportedException("OnComplete is not supported");
                }
            };
            onError += delegate(Http.HttpConnection sender, string message, Exception exception)
            {
                Logger.Storage.Error("An error occurred while processing the command.  Message: " + message, exception);
                if (OnError != null) OnError(this, _client, message, exception);
                else throw new UnsupportedException("OnError is not supported");
            };
            onProgress += delegate(Http.HttpConnection sender, Tcp.DirectionType direction, int packetSize, decimal requestPercentSent, decimal responsePercentReceived)
            {
                Logger.Storage.Debug("Progress made on command (send at " + requestPercentSent.ToString() + "%, receive at " + responsePercentReceived.ToString() + "%)");
                if (OnProgress != null) OnProgress(this, _client, sender, direction, packetSize, requestPercentSent, responsePercentReceived);
            };



            Logger.Storage.Debug("Begining command execution for " + GetType().FullName + "...");

            _client.Execute(_httpRequest, new Tcp.Params.Buffer() { Size = receiveBufferSize, Timeout = receiveTimeout },
                new Tcp.Params.Buffer() { Size = sendBufferSize, Timeout = sendTimeout },
                onConnect, onDisconnect, onError, onProgress, onTimeout, onComplete);
        }

        public virtual void Close()
        {
            Logger.Storage.Debug("Closing client...");

            //try { _client.Close(); }
            //catch (Exception e)
            //{
            //    Logger.Storage.Error("An exception occurred while closing the client.", e);
            //    throw;
            //}
        }

        public abstract ReplyBase MakeReply(Http.Response response);
    }
}
