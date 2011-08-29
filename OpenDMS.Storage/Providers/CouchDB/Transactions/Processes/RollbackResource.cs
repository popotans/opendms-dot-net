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
        private Dictionary<Data.VersionId, Data.Version> _versionsToDelete;
        private int _receivedCount;

        public RollbackResource(IDatabase db, Data.ResourceId resourceId, int rollbackDepth,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _resourceId = resourceId;
            _rollbackDepth = rollbackDepth;
            _requestingPartyType = requestingPartyType;
            _session = session;
            _receivedCount = 0;
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
            else if (t == typeof(Tasks.CheckResourcePermissions))
            {
                Data.VersionId oldCurrentVersionId;
                Tasks.CheckResourcePermissions task = (Tasks.CheckResourcePermissions)sender;

                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }

                // If version number < rollback -> error
                // Versions: 0,1,2 / rollback: 2 -> 0th version
                if (_resource.CurrentVersionId.VersionNumber < _rollbackDepth)
                {
                    TriggerOnError(null, "Rollback depth out of range.", null);
                    return;
                }
                else if (_rollbackDepth <= 0)
                {
                    TriggerOnError(null, "Rollback depth must be a positive value.", null);
                    return;
                }

                oldCurrentVersionId = _resource.CurrentVersionId;

                _versionsToDelete = new Dictionary<Data.VersionId, Data.Version>();

                long targetVersionNumber = _resource.CurrentVersionId.VersionNumber - _rollbackDepth;
                for(int i=0; i<_resource.VersionIds.Count; i++)
                {
                    if (_resource.VersionIds[i].VersionNumber > targetVersionNumber)
                    {                        
                        Data.VersionId vid = new Data.VersionId(_resource.ResourceId, i);
                        _versionsToDelete.Add(vid, new Data.Version(vid));
                    }
                }
                
                // Removes the versions more recent than the rollback point
                _resource.VersionIds.RemoveRange(_resource.VersionIds.Count - _rollbackDepth,
                    _rollbackDepth);

                _resource.UpdateCurrentVersionBasedOnVersionsList();


                Dictionary<Data.VersionId, Data.Version>.Enumerator en = _versionsToDelete.GetEnumerator();
                
                // Dispatch all our heads to get the revisions
                // *note* do not combine these loops - we want the full list before starting
                while (en.MoveNext())
                {
                    RunTaskProcess(new Tasks.HeadVersion(_db, en.Current.Key, _sendTimeout,
                        _receiveTimeout, _sendBufferSize, _receiveBufferSize));
                }
            }
            else if (t == typeof(Tasks.HeadVersion))
            {
                Tasks.HeadVersion task = (Tasks.HeadVersion)sender;

                if (!_versionsToDelete.ContainsKey(task.VersionId))
                {
                    TriggerOnError(task, "The id '" + task.VersionId.ToString() + "' could not be found.", new KeyNotFoundException());
                    return;
                }

                lock (_versionsToDelete)
                {
                    _receivedCount++;
                    _versionsToDelete[task.VersionId].UpdateRevision(task.Revision);
                    if (_versionsToDelete.Count == _receivedCount)
                    {
                        // Inside here we have a collection "docs" that contains the new resource
                        // which has the new "current" version and has all the more recent
                        // versions removed.  We also have inside "docs" deletion markers for all
                        // the more recent versions.

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
                        
                        Dictionary<Data.VersionId, Data.Version>.Enumerator en = _versionsToDelete.GetEnumerator();
                
                        // Dispatch all our heads to get the revisions
                        // *note* do not combine these loops - we want the full list before starting
                        while (en.MoveNext())
                        {
                            Transitions.Version txVersion = new Transitions.Version();
                            Model.Document doc2 = txVersion.Transition(en.Current.Value, out errors);
                            if (errors != null)
                            {
                                TriggerOnError(null, errors[0].Message, errors[0]);
                                return;
                            }
                            doc2.Add("_deleted", true);
                            docs.Add(doc2);
                        }
                        
                        RunTaskProcess(new Tasks.MakeBulkDocument(docs, _sendTimeout, _receiveTimeout,
                            _sendBufferSize, _receiveBufferSize));
                    }
                }
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
