using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DownloadGlobalPermissions : Base
    {
        private IDatabase _db;
        private GlobalUsageRights _gur;

        public List<Security.UsageRight> UsageRights { get { return _gur.UsageRights; } }

        public DownloadGlobalPermissions(IDatabase db)
        {
            _db = db;
            _gur = new GlobalUsageRights(null, null);
        }

        public override void Process()
        {
            Remoting.Get rem;

            TriggerOnActionChanged(EngineActionType.GettingGlobalUsageRights, true);

            try
            {
                rem = new Remoting.Get(_db, _gur.Id);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Get object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                Transitions.GlobalUsageRights txGur = new Transitions.GlobalUsageRights();
                _gur = (GlobalUsageRights)txGur.Transition(((Remoting.Get)sender).Document);
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
