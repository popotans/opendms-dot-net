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

        public abstract void Process();

        public Base()
        {
        }

        protected void TriggerOnActionChanged(EngineActionType actionType, bool willSendProgress)
        {
            try
            {
                if (OnActionChanged != null) OnActionChanged(this, actionType, willSendProgress);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }
        }

        protected void TriggerOnAuthorizationDenied()
        {
            if (OnAuthorizationDenied == null)
                throw new NotImplementedException("OnAuthorizationDenied must be implemented.");

            try
            {
                OnAuthorizationDenied(this);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnAuthorizationDenied event.", e);
                throw;
            }
        }

        protected void TriggerOnComplete(ICommandReply reply)
        {
            if (OnComplete == null)
                throw new NotImplementedException("OnComplete must be implemented.");

            try
            {
                OnComplete(this, reply);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnComplete event.", e);
                throw;
            }
        }

        protected void TriggerOnError(string message, Exception exception)
        {
            if (OnError == null)
                throw new NotImplementedException("OnError must be implemented.");

            try
            {
                OnError(this, message, exception);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnError event.", e);
                throw;
            }
        }

        protected void TriggerOnProgress(OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            try
            {
                if (OnProgress != null) OnProgress(this, direction, packetSize, sendPercentComplete, receivePercentComplete);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnProgress event.", e);
                throw;
            }
        }

        protected void TriggerOnTimeout()
        {
            try
            {
                if (OnTimeout != null) OnTimeout(this);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnTimeout event.", e);
                throw;
            }
        }
    }
}
