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

        protected Model.Document _document;

        public abstract Model.Document Execute();
        public abstract void Commit();

        public Base(Model.Document document)
        {
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
    }
}
