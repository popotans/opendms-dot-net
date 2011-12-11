using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DownloadResource : Base
    {
        private IDatabase _db;
        private Data.ResourceId _id;

        public Data.Resource Resource { get; private set; }
        public JObject Remainder { get; private set; }

        public DownloadResource(IDatabase db, Data.ResourceId id,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _id = id;
        }

        public override void Process()
        {
            Remoting.Get rem;

            TriggerOnActionChanged(EngineActionType.GettingResource, true);

            try
            {
                rem = new Remoting.Get(_db, _id.ToString(), _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Get object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                JObject jobj;
                Transitions.Resource txResource = new Transitions.Resource();
                Resource = txResource.Transition(((Remoting.Get)sender).Document, out jobj);
                Remainder = jobj;
                TriggerOnComplete(reply);
            };
            rem.OnError += delegate(Remoting.Base sender, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            rem.OnProgress += delegate(Remoting.Base sender, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                TriggerOnProgress(direction, packetSize, sendPercentComplete, receivePercentComplete);
            };
            rem.OnTimeout += delegate(Remoting.Base sender)
            {
                TriggerOnTimeout();
            };

            rem.Process();
        }
    }
}
