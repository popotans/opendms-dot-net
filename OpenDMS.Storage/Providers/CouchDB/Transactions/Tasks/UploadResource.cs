using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadResource : Base
    {
        private IDatabase _db;
        private Data.Resource _resource;
        private JObject _jobj;

        public Data.Resource Resource { get; private set; }
        public Model.Document Document { get; private set; }

        public UploadResource(IDatabase db, Data.Resource resource,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _resource = resource;
        }

        public UploadResource(IDatabase db, JObject jobj,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _jobj = jobj;
        }

        public override void Process()
        {
            if (_resource != null)
                ProcessResource();
            else if (_jobj != null)
                ProcessJObject();
            else
                TriggerOnError("Invalid state.", new InvalidOperationException("Either a Data.Resource or JObject must be provided."));
        }

        public void ProcessResource()
        {
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

            if (errors != null)
            {
                for (int i = 0; i < errors.Count; i++)
                    Logger.Storage.Error("Error encountered in transitioning from resource to document.", errors[i]);
                TriggerOnError(errors[0].Message, errors[0]);
                return;
            }

            ProcessCommon(doc);
        }

        public void ProcessJObject()
        {
            // A new resource will have a null revision
            if (_jobj["_rev"] == null)
                TriggerOnActionChanged(EngineActionType.CreatingNewResource, true);
            else
                TriggerOnActionChanged(EngineActionType.ModifyingResource, true);

            ProcessCommon(new Model.Document(_jobj));
        }

        private void ProcessCommon(Model.Document doc)
        {
            Remoting.SaveSingle rem;

            try
            {
                rem = new Remoting.SaveSingle(_db, doc, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
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
                    if (_resource != null)
                    {
                        Resource = _resource;
                        Resource.UpdateRevision(((Commands.PutDocumentReply)reply).Rev);
                    }
                    if (_jobj != null)
                    {
                        doc.Rev = ((Commands.PutDocumentReply)reply).Rev;
                        Document = doc;
                    }
                }
                TriggerOnComplete(reply);
            };
            rem.OnError += delegate(Remoting.Base sender, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            rem.OnProgress += delegate(Remoting.Base sender, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
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
