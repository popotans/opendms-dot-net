using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public abstract class Base
    {
        public delegate void ActionDelegate(Base sender, EngineActionType actionType, bool willSendProgress);
        public delegate void AuthorizationDelegate(Base sender);
        public delegate void CompletionDelegate(Base sender, ICommandReply reply);
        public delegate void ErrorDelegate(Base sender, string message, Exception exception);
        public delegate void ProgressDelegate(Base sender, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void TimeoutDelegate(Base sender);

        public event ActionDelegate OnActionChanged;
        public event AuthorizationDelegate OnAuthorizationDenied;
        public event CompletionDelegate OnComplete;
        public event ErrorDelegate OnError;
        public event ProgressDelegate OnProgress;
        public event TimeoutDelegate OnTimeout;

        public Base()
        {
        }

        protected void TriggerOnActionChanged(EngineActionType actionType, bool willSendProgress)
        {
            if (OnActionChanged != null) OnActionChanged(this, actionType, willSendProgress);
        }

        protected void TriggerOnAuthorizationDenied()
        {
            if (OnAuthorizationDenied == null)
                throw new NotImplementedException("OnAuthorizationDenied must be implemented.");

            OnAuthorizationDenied(this);
        }

        protected void TriggerOnComplete(ICommandReply reply)
        {
            if (OnComplete == null)
                throw new NotImplementedException("OnComplete must be implemented.");

            OnComplete(this, reply);
        }

        protected void TriggerOnError(string message, Exception exception)
        {
            if (OnError == null)
                throw new NotImplementedException("OnError must be implemented.");

            OnError(this, message, exception);
        }

        protected void TriggerOnProgress(OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (OnProgress != null) OnProgress(this, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

        protected void TriggerOnTimeout()
        {
            if (OnTimeout != null) OnTimeout(this);
        }
    }
}
