using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class DeleteResource : Base
    {
        private Data.ResourceId _id;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private Data.Resource _resource;
        private JObject _remainder;

        public DeleteResource(IDatabase db, Data.ResourceId id,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _id = id;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadResource(_db, _id, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadResource))
            {
                Tasks.DownloadResource task = (Tasks.DownloadResource)sender;
                _resource = task.Resource;
                _remainder = task.Remainder;
                RunTaskProcess(new Tasks.CheckResourcePermissions(_db, _resource, _requestingPartyType,
                    _session, Security.Authorization.ResourcePermissionType.Delete,
                    _sendTimeout, _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.CheckGlobalPermissions))
            {
                Tasks.CheckGlobalPermissions task = (Tasks.CheckGlobalPermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                
                List<Exception> errors;
                List<Model.Document> docs = new List<Model.Document>();
                Transitions.Resource txResource = new Transitions.Resource();
                Model.Document doc = txResource.Transition(_resource, out errors);

                if (errors != null)
                {
                    TriggerOnError(null, errors[0].Message, errors[0]);
                    return;
                }

                doc.CombineWith(_remainder);
                docs.Add(doc);

                for (long i = 0; 
                    i > _resource.CurrentVersionId.VersionNumber; i++)
                {
                    Data.Version v = new Data.Version(new Data.VersionId(_resource.ResourceId, i));
                    v.Metadata.Add("_deleted", true);
                    Transitions.Version txVersion = new Transitions.Version();
                    Model.Document doc2 = txVersion.Transition(v, out errors);
                    if (errors != null)
                    {
                        TriggerOnError(null, errors[0].Message, errors[0]);
                        return;
                    }
                    docs.Add(doc2);
                }

                RunTaskProcess(new Tasks.MakeBulkDocument(docs, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.MakeBulkDocument))
            {
                Tasks.MakeBulkDocument task = (Tasks.MakeBulkDocument)sender;
                RunTaskProcess(new Tasks.UploadBulkDocuments(_db, task.BulkDocument, _sendTimeout,
                    _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.UploadBulkDocuments))
            {
                Tasks.UploadBulkDocuments task = (Tasks.UploadBulkDocuments)sender;
                TriggerOnComplete(reply, task.Results);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
