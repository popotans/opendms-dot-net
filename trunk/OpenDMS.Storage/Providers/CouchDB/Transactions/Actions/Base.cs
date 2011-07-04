using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Actions
{
    public abstract class Base
    {
        public delegate void CommitProgressDelegate(Base sender, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void CommitCompletionDelegate(Base sender, ICommandReply reply);
        public delegate void CommitTimeoutDelegate(Base sender);
        public delegate void CommitErrorDelegate(Base sender, string message, Exception exception);

        public event CommitCompletionDelegate OnComplete;
        public event CommitErrorDelegate OnError;
        public event CommitProgressDelegate OnProgress;
        public event CommitTimeoutDelegate OnTimeout;

        protected IDatabase _db;
        protected Model.Document _document;

        public abstract Model.Document Execute();
        public abstract void Commit();

        public Base(IDatabase db, Model.Document document)
        {
            _db = db;
            _document = document;
        }

        protected void TriggerOnComplete(ICommandReply reply)
        {
            if (OnComplete != null) OnComplete(this, reply);
        }

        protected void TriggerOnError(string message, Exception exception)
        {
            if (OnError != null) OnError(this, message, exception);
        }

        protected void TriggerOnProgress(int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (OnProgress != null) OnProgress(this, packetSize, sendPercentComplete, receivePercentComplete);
        }

        protected void TriggerOnTimeout()
        {
            if (OnTimeout != null) OnTimeout(this);
        }

        protected virtual void Commit_OnTimeout(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection)
        {
            TriggerOnTimeout();
        }

        protected virtual void Commit_OnError(Commands.Base sender, Networking.Http.Client client, string message, System.Exception exception)
        {
            TriggerOnError(message, exception);
        }

        protected virtual void Commit_OnComplete(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Commands.ReplyBase reply)
        {
            TriggerOnComplete(reply);
        }

        protected virtual void Commit_OnProgress(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            TriggerOnProgress(packetSize, sendPercentComplete, receivePercentComplete);
        }
    }
}
