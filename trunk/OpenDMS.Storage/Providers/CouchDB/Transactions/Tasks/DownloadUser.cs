using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DownloadUser : Base
    {
        private IDatabase _db;
        private Security.User _user;

        public Security.User User { get; private set; }

        public DownloadUser(IDatabase db, string username)
        {
            _db = db;
            _user = new Security.User(username);
        }

        public override void Process()
        {
            Remoting.Get rem;

            TriggerOnActionChanged(EngineActionType.GettingUser, true);

            try
            {
                rem = new Remoting.Get(_db, _user.Id);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Get object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                Transitions.User txUser = new Transitions.User();
                User = txUser.Transition(((Remoting.Get)sender).Document);
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
