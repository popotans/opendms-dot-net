using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public abstract class Base
    {
        public delegate void ActionDelegate(Base sender, Tasks.Base task, EngineActionType actionType, bool willSendProgress);
        public delegate void AuthorizationDelegate(Base sender, Tasks.Base task);
        public delegate void CompletionDelegate(Base sender, ICommandReply reply, object result);
        public delegate void ErrorDelegate(Base sender, Tasks.Base task, string message, Exception exception);
        public delegate void ProgressDelegate(Base sender, Tasks.Base task, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void TimeoutDelegate(Base sender, Tasks.Base task);
        public delegate void TaskCompletionDelegate(Base sender, Tasks.Base task);

        public event ActionDelegate OnActionChanged;
        public event AuthorizationDelegate OnAuthorizationDenied;
        public event CompletionDelegate OnComplete;
        public event ErrorDelegate OnError;
        public event ProgressDelegate OnProgress;
        public event TimeoutDelegate OnTimeout;
        public event TaskCompletionDelegate OnTaskComplete;

        protected IDatabase _db;
        protected int _sendTimeout;
        protected int _receiveTimeout;
        protected int _sendBufferSize;
        protected int _receiveBufferSize;

        public IDatabase Database { get { return _db; } private set { _db = value; } }
        
        public Base(IDatabase db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            _db = db;
            _sendTimeout = sendTimeout;
            _sendBufferSize = sendBufferSize;
            _receiveTimeout = receiveTimeout;
            _receiveBufferSize = receiveBufferSize;
        }

        public abstract void Process();
        public abstract void TaskComplete(Tasks.Base sender, ICommandReply reply);


        protected virtual void RunTaskProcess(Tasks.Base task)
        {
            task.OnActionChanged += delegate(Tasks.Base sender, EngineActionType actionType, bool willSendProgress)
            {
                TriggerOnActionChanged(sender, actionType, willSendProgress);
            };
            task.OnAuthorizationDenied += delegate(Tasks.Base sender)
            {
                TriggerOnAuthorizationDenied(sender);
            };
            task.OnComplete += TaskComplete;
            task.OnError += delegate(Tasks.Base sender, string message, Exception exception)
            {
                TriggerOnError(sender, message, exception);
            };
            task.OnProgress += delegate(Tasks.Base sender, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                TriggerOnProgress(sender, direction, packetSize, sendPercentComplete, receivePercentComplete);
            };
            task.OnTimeout += delegate(Tasks.Base sender)
            {
                TriggerOnTimeout(sender);
            };

            task.Process();
        }

        protected void TriggerOnActionChanged(Tasks.Base task, EngineActionType actionType, bool willSendProgress)
        {
            try
            {
                if (OnActionChanged != null) OnActionChanged(this, task, actionType, willSendProgress);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }
        }

        protected void TriggerOnAuthorizationDenied(Tasks.Base task)
        {
            if (OnAuthorizationDenied == null)
                throw new NotImplementedException("OnAuthorizationDenied must be implemented.");

            try
            {
                OnAuthorizationDenied(this, task);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnAuthorizationDenied event.", e);
                throw;
            }
        }

        protected void TriggerOnComplete(ICommandReply reply, object result)
        {
            if (OnComplete == null)
                throw new NotImplementedException("OnComplete must be implemented.");

            try
            {
                OnComplete(this, reply, result);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnComplete event.", e);
                throw;
            }
        }

        protected void TriggerOnError(Tasks.Base task, string message, Exception exception)
        {
            if (OnError == null)
                throw new NotImplementedException("OnError must be implemented.");

            try
            {
                OnError(this, task, message, exception);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnError event.", e);
                throw;
            }
        }

        protected void TriggerOnProgress(Tasks.Base task, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            try
            {
                if (OnProgress != null)
                    OnProgress(this, task, direction, packetSize, sendPercentComplete, receivePercentComplete);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnProgress event.", e);
                throw;
            }
        }

        protected void TriggerOnTimeout(Tasks.Base task)
        {
            try
            {
                if (OnTimeout != null) OnTimeout(this, task);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnTimeout event.", e);
                throw;
            }
        }
    }
}
