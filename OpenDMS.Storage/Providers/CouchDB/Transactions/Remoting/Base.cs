using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public abstract class Base
    {
        public delegate void CompletionDelegate(Base sender, ICommandReply reply);
        public delegate void ErrorDelegate(Base sender, string message, Exception exception);
        public delegate void ProgressDelegate(Base sender, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void TimeoutDelegate(Base sender);

        public event CompletionDelegate OnComplete;
        public event ErrorDelegate OnError;
        public event ProgressDelegate OnProgress;
        public event TimeoutDelegate OnTimeout;

        protected Uri _uri = null;
        protected IDatabase _db;
        protected string _id;
        protected JObject _input;
        protected int _sendTimeout;
        protected int _receiveTimeout;
        protected int _sendBufferSize;
        protected int _receiveBufferSize;

        public ICommandReply Reply { get; protected set; }

        public abstract void Process();

        public Base(IDatabase db, string id, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            _uri = UriBuilder.Build(db, id);
            _db = db;
            _id = id;
            _sendTimeout = sendTimeout;
            _sendBufferSize = sendBufferSize;
            _receiveTimeout = receiveTimeout;
            _receiveBufferSize = receiveBufferSize;
        }

        protected void TriggerOnComplete(ICommandReply reply)
        {
            if (OnComplete == null)
                throw new NotImplementedException("OnComplete must be implemented.");

            Reply = reply;
            OnComplete(this, reply);
        }

        protected void TriggerOnError(string message, Exception exception)
        {
            if (OnError == null)
                throw new NotImplementedException("OnError must be implemented.");

            OnError(this, message, exception);
        }

        protected void TriggerOnProgress(OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (OnProgress != null) OnProgress(this, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

        protected void TriggerOnTimeout()
        {
            if (OnTimeout != null) OnTimeout(this);
        }
    }
}
