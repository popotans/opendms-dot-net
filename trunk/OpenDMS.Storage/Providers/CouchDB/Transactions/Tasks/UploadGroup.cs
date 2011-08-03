using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadGroup : Base
    {
        private IDatabase _db;
        private Security.Group _group;

        public Security.Group Group { get; private set; }

        public UploadGroup(IDatabase db, Security.Group group)
        {
            _db = db;
            _group = group;
        }

        public override void Process()
        {
            Remoting.SaveSingle rem;

            if (string.IsNullOrEmpty(_group.Rev))
                TriggerOnActionChanged(EngineActionType.CreatingGroup, true);
            else
                TriggerOnActionChanged(EngineActionType.ModifyingGroup, true);

            try
            {
                Transitions.Group txGroup = new Transitions.Group();
                rem = new Remoting.SaveSingle(_db, txGroup.Transition(_group));
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.SaveSingle object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                if (!((Commands.PutDocumentReply)reply).Ok)
                    Group = null;
                else
                {
                    Group = new Security.Group(_group.Id, ((Commands.PutDocumentReply)reply).Rev,
                        _group.Users, _group.Groups);
                }
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
