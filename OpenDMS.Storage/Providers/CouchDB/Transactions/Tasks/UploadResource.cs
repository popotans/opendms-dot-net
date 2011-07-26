using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadResource : Base
    {
        private IDatabase _db;
        private Data.Resource _resource;

        public Data.Resource Resource { get; private set; }

        public UploadResource(IDatabase db, Data.Resource resource)
        {
            _db = db;
            _resource = resource;
        }

        public override void Process()
        {
            Remoting.SaveSingle rem;
            Model.Document doc;
            Transitions.Resource txResource;
            List<Exception> errors;

            // A new resource will have a null revision
            if (string.IsNullOrEmpty(_resource.Revision))
                TriggerOnActionChanged(EngineActionType.CreatingNewResource, true);
            else
                TriggerOnActionChanged(EngineActionType.ModifyingResource, true);

            txResource = new Transitions.Resource();
            doc = txResource.Transition(_resource, out errors);

            if (errors.Count > 0)
            {
                for (int i = 0; i < errors.Count; i++)
                    Logger.Storage.Error("Error encountered in transitioning from resource to document.", errors[i]);
                TriggerOnError(errors[0].Message, errors[0]);
                return;
            }

            try
            {
                rem = new Remoting.SaveSingle(_db, doc);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Get object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                if (!((Commands.PutDocumentReply)reply).Ok)
                    Resource = null;
                else
                {
                    Resource = _resource;
                    Resource.UpdateRevision(((Commands.PutDocumentReply)reply).Rev);
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
