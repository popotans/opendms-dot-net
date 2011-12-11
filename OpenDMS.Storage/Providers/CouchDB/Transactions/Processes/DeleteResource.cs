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
        private Dictionary<Data.VersionId, Data.Version> _versions;
        private int _receivedCount;

        public DeleteResource(IDatabase db, Data.ResourceId id,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _id = id;
            _requestingPartyType = requestingPartyType;
            _session = session;
            _versions = new Dictionary<Data.VersionId, Data.Version>();
            _receivedCount = 0;
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
            else if (t == typeof(Tasks.CheckResourcePermissions))
            {
                Tasks.CheckResourcePermissions task = (Tasks.CheckResourcePermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }

                // First, we load up revisions
                for (int i = 0; i < _resource.VersionIds.Count; i++)
                {
                    _versions.Add(_resource.VersionIds[i], new Data.Version(_resource.VersionIds[i]));
                }

                // Now our _revisions holds indexes for all version ids
                // Dispatch all our heads to get the revisions
                // *note* do not combine these loops - we want the full dictionary before starting
                for (int i = 0; i < _resource.VersionIds.Count; i++)
                {
                    RunTaskProcess(new Tasks.HeadVersion(_db, _resource.VersionIds[i], _sendTimeout,
                        _receiveTimeout, _sendBufferSize, _receiveBufferSize));
                }
            }
            else if (t == typeof(Tasks.HeadVersion))
            {
                Tasks.HeadVersion task = (Tasks.HeadVersion)sender;

                if (!_versions.ContainsKey(task.VersionId))
                {
                    TriggerOnError(task, "The id '" + task.VersionId.ToString() + "' could not be found.", new KeyNotFoundException());
                    return;
                }

                lock (_versions)
                {
                    _receivedCount++;
                    _versions[task.VersionId].UpdateRevision(task.Revision);
                    if (_versions.Count == _receivedCount)
                    {
                        List<Exception> errors;
                        List<Model.Document> docs = new List<Model.Document>();
                        Transitions.Resource txResource = new Transitions.Resource();
                        Model.Document doc = txResource.Transition(_resource, out errors);

                        if (errors != null)
                        {
                            TriggerOnError(null, errors[0].Message, errors[0]);
                            return;
                        }

                        // Pointless, we are deleting it
                        //doc.CombineWith(_remainder);
                        doc.Add("_deleted", true);
                        docs.Add(doc);

                        Dictionary<Data.VersionId, Data.Version>.Enumerator en = _versions.GetEnumerator();

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
                    else
                    {
                        TriggerOnProgress(task, Networking.Protocols.Tcp.DirectionType.Download, -1, -1,
                            ((decimal)((decimal)_receivedCount / (decimal)_versions.Count)));
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
