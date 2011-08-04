using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DownloadGlobalPermissions : Base
    {
        private IDatabase _db;

        public GlobalUsageRights GlobalUsageRights { get; private set; }

        public DownloadGlobalPermissions(IDatabase db,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
        }

        public override void Process()
        {
            Remoting.Get rem;

            TriggerOnActionChanged(EngineActionType.GettingGlobalUsageRights, true);

            try
            {
                rem = new Remoting.Get(_db, new GlobalUsageRights(null, null).Id, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Get object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                Transitions.GlobalUsageRights txGur = new Transitions.GlobalUsageRights();
                GlobalUsageRights = (GlobalUsageRights)txGur.Transition(((Remoting.Get)sender).Document);
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
