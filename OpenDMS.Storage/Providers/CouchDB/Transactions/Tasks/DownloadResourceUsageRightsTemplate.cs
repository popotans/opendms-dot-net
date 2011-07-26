using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class DownloadResourceUsageRightsTemplate : Base
    {
        private const string _id = "resourceusagerightstemplate";
        private IDatabase _db;

        public ResourceUsageRightsTemplate Value { get; private set; }

        public DownloadResourceUsageRightsTemplate(IDatabase db)
        {
            _db = db;
        }

        public void Process()
        {
            Remoting.Get rem;

            TriggerOnActionChanged(EngineActionType.GettingResourceUsageRightsTemplate, true);

            rem = new Remoting.Get(_db, _id);

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                Transitions.ResourceUsageRights txRur = new Transitions.ResourceUsageRights();
                Value = (ResourceUsageRightsTemplate)txRur.Transition(((Remoting.Get)sender).Document);
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
