using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class RollbackResource : Base
    {
        private Data.ResourceId _resourceId;
        private int _rollbackDepth;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private Data.Resource _resource;
        private JObject _remainder;

        public RollbackResource(IDatabase db, Data.ResourceId resourceId, int rollbackDepth,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _resourceId = resourceId;
            _rollbackDepth = rollbackDepth;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadResource(_db, _resourceId, _sendTimeout, _receiveTimeout,
                _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            /* We encounter a bit of an issue here.  Upon doing some research it is easily resolved
             * by couchdb.  Lets examine.
             * 
             * Issue: We update the resource to the server, connection is lost, we try to delete the
             * versions newer than the target version of the rollback.  This fails.  We now have 
             * zombie versions that will block future recreation of those versions as the IDs will
             * be the same.
             * 
             * Solution: "Updating existing documents requires setting the _rev member to the 
             * revision being updated. To delete a document set the _deleted member to true." 
             * http://wiki.apache.org/couchdb/HTTP_Bulk_Document_API.
             * 
             * Example: 
             * {
             *  "docs": [
             *      {"_id": "0", "_rev": "1-62657917", "_deleted": true},
             *      {"_id": "1", "_rev": "1-2089673485", "integer": 2, "string": "2"},
             *      {"_id": "2", "_rev": "1-2063452834", "integer": 3, "string": "3"}
             *  ]
             * }
             * 
             * Implementation: So, we already have bulk document post support.  We simply need to
             * load the bulkdocument with the resource and the _id and _rev for each version we want
             * to remove.  We will also want to set the _deleted to true.
             */

            // 1) Download Resource
            // 2) Check for VersionControl permission
            // 3) Modify Resource (VersionIds, CurrentVersionId)
            // 4) Make bulk document
            // 5) Upload bulk document
            
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadResource))
            {
                Tasks.DownloadResource task = (Tasks.DownloadResource)sender;
                _resource = task.Resource;
                _remainder = task.Remainder;
                RunTaskProcess(new Tasks.CheckResourcePermissions(_db, _resource, _requestingPartyType,
                    _session, Security.Authorization.ResourcePermissionType.VersionControl, 
                    _sendTimeout, _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.CheckGlobalPermissions))
            {
                Data.VersionId oldCurrentVersionId;

                Tasks.CheckGlobalPermissions task = (Tasks.CheckGlobalPermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                
                // Modify Resource
                // If version number < rollback -> error
                // Versions: 0,1,2 / rollback: 2 -> 0th version
                if (_resource.CurrentVersionId.VersionNumber < _rollbackDepth)
                {
                    TriggerOnError(null, "Rollback depth out of range.", null);
                }
                else if (_rollbackDepth <= 0)
                {
                    TriggerOnError(null, "Rollback depth must be a positive value.", null);
                }

                oldCurrentVersionId = _resource.CurrentVersionId;

                // Removes the versions more recent than the rollback point
                _resource.VersionIds.RemoveRange(_resource.VersionIds.Count - _rollbackDepth, 
                    _resource.VersionIds.Count);

                _resource.UpdateCurrentVersionBasedOnVersionsList();

                // Now resource is all patched up.

                // So, we need to make the bulk document with the new resource and the deletion 
                // documents for all the versions after the rollback point.
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

                for (long i = oldCurrentVersionId.VersionNumber; 
                    i > _resource.CurrentVersionId.VersionNumber; i--)
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
