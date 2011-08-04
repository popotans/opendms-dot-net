using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadUser : Base
    {
        private IDatabase _db;
        private Security.User _user;

        public Security.User User { get; private set; }

        public UploadUser(IDatabase db, Security.User user,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _user = user;
        }

        public override void Process()
        {
            Remoting.SaveSingle rem;

            if (string.IsNullOrEmpty(_user.Rev))
                TriggerOnActionChanged(EngineActionType.CreatingGroup, true);
            else
                TriggerOnActionChanged(EngineActionType.ModifyingGroup, true);

            try
            {
                Transitions.User txUser = new Transitions.User();
                rem = new Remoting.SaveSingle(_db, txUser.Transition(_user), _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.SaveSingle object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                if (!((Commands.PutDocumentReply)reply).Ok)
                    User = null;
                else
                {
                    User = new Security.User(_user.Id, ((Commands.PutDocumentReply)reply).Rev,
                        null, _user.FirstName, _user.MiddleName, _user.LastName, _user.Groups,
                        _user.IsSuperuser);
                    User.SetEncryptedPassword(_user.Password);
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
