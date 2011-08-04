using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DownloadGroups : Base
    {
        private IDatabase _db;

        public List<Security.Group> Groups { get; private set; }

        public DownloadGroups(IDatabase db,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
        }

        public override void Process()
        {
            Remoting.GetView rem;

            TriggerOnActionChanged(EngineActionType.GettingGroups, true);

            try
            {
                rem = new Remoting.GetView(_db, "groups", "GetAll", _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.GetView object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                Transitions.GroupCollection txGc = new Transitions.GroupCollection();
                Groups = txGc.Transition(rem.View);
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
