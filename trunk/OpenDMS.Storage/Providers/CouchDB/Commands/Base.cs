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

            Logger.Storage.Debug("Begining command execution for " + GetType().FullName + "...");

            try
            {
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
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
                throw;
            }
        }

        public virtual void Close()
        {
            Logger.Storage.Debug("Closing client...");

            try { _client.Close(); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while closing the client.", e);
                throw;
            }
        }

        protected virtual void Client_OnClose(Client sender, Connection connection)
        {
            Logger.Storage.Debug("Client closed.");
            if (OnClose != null) OnClose(this, sender, connection);
        }

        protected virtual void Client_OnTimeout(Client sender, Connection connection)
        {
            Logger.Storage.Error("The command timed out.");
            if (OnTimeout != null) OnTimeout(this, sender, connection);
            else throw new UnsupportedException("OnTimeout is not supported");
        }

        protected virtual void Client_OnProgress(Client sender, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            Logger.Storage.Debug("Progress made on command (send at " + sendPercentComplete.ToString() + "%, receive at " + receivePercentComplete.ToString() + "%)");
            if (OnProgress != null) OnProgress(this, sender, connection, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

        protected virtual void Client_OnError(Client sender, string message, Exception exception)
        {
            Logger.Storage.Error("An error occurred while processing the command.  Message: " + message, exception);
            if (OnError != null) OnError(this, sender, message, exception);
            else throw new UnsupportedException("OnError is not supported");
        }

        protected virtual void Client_OnComplete(Client sender, Connection connection, Response response)
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
                OnError(this, sender, "An exception occurred while attempting to create the reply object.", e);
            }

            if (reply != null)
            {
                if (OnComplete != null) OnComplete(this, sender, connection, reply);
                else throw new UnsupportedException("OnComplete is not supported");
            }
        }

        public abstract ReplyBase MakeReply(Response response);
    }
}
