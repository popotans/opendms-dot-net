using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DetermineIfExists : Base
    {
        private IDatabase _db;
        private string _id;

        public bool Exists { get; private set; }

        public DetermineIfExists(IDatabase db, string id,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _id = id;
        }

        public override void Process()
        {
            Remoting.Exists rem;

            TriggerOnActionChanged(EngineActionType.CheckingExistance, true);

            try
            {
                rem = new Remoting.Exists(_db, _id, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Get object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                Exists = rem.IsExisting;
                TriggerOnComplete(reply);
            };
            rem.OnError += delegate(Remoting.Base sender, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            rem.OnProgress += delegate(Remoting.Base sender, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
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
