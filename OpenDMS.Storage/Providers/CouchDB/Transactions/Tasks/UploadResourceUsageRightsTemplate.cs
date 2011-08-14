using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadResourceUsageRightsTemplate : Base
    {
        private const string _id = "resourceusagerightstemplate";
        private IDatabase _db;

        public ResourceUsageRightsTemplate Value { get; private set; }

        public UploadResourceUsageRightsTemplate(IDatabase db, ResourceUsageRightsTemplate template,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            Value = template;
            _db = db;
        }

        public override void Process()
        {
            Remoting.SaveSingle rem;

            TriggerOnActionChanged(EngineActionType.GettingResourceUsageRightsTemplate, true);

            try
            {
                Transitions.ResourceUsageRights txRur = new Transitions.ResourceUsageRights();
                rem = new Remoting.SaveSingle(_db, txRur.Transition(Value), _sendTimeout, _receiveTimeout,
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
                    Value = null;
                else
                {
                    Value = new ResourceUsageRightsTemplate(((Commands.PutDocumentReply)reply).Rev,
                        Value.UsageRights);
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
