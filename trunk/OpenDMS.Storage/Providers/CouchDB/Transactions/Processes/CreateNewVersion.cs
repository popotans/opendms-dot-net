using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class CreateNewVersion : Base
    {
        private Data.Version _version;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private GlobalUsageRights _gur;
        private Data.Resource _resource;
        private JObject _resourceRemainder;

        public CreateNewVersion(IDatabase db, Data.Version version,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _version = version;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadResource(_db, _version.VersionId.ResourceId, _sendTimeout, 
                _receiveTimeout, _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadResource))
            {
                Tasks.DownloadResource task = (Tasks.DownloadResource)sender;
                _resource = task.Resource;
                _resourceRemainder = task.Remainder;
                RunTaskProcess(new Tasks.CheckResourcePermissions(_db, _resource, _requestingPartyType,
                    _session, Security.Authorization.ResourcePermissionType.Checkout, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.CheckResourcePermissions))
            {
                Tasks.CheckResourcePermissions task = (Tasks.CheckResourcePermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                RunTaskProcess(new Tasks.MarkResourceForCheckout(_resource, _session.User.Username, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.MarkResourceForCheckout))
            {
                Tasks.MarkResourceForCheckout task = (Tasks.MarkResourceForCheckout)sender;
                _resource = task.Resource;

                // Update versionid of version
                _version.UpdateVersionNumber(_resource.CurrentVersionId.VersionNumber + 1);
                _resource.VersionIds.Add(_version.VersionId);
                _resource.UpdateCurrentVersionBasedOnVersionsList();

                List<Exception> errors;
                List<Model.Document> docs = new List<Model.Document>();
                Transitions.Version txVersion = new Transitions.Version();
                Transitions.Resource txResource = new Transitions.Resource();

                docs.Add(txResource.Transition(_resource, out errors));
                if (errors != null)
                {
                    TriggerOnError(null, errors[0].Message, errors[0]);
                    return;
                }
                docs[0].CombineWith(_resourceRemainder);
                docs.Add(txVersion.Transition(_version, out errors));
                if (errors != null)
                {
                    TriggerOnError(null, errors[0].Message, errors[0]);
                    return;
                }

                RunTaskProcess(new Tasks.MakeBulkDocument(docs, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.MakeBulkDocument))
            {
                Tasks.MakeBulkDocument task = (Tasks.MakeBulkDocument)sender;
                RunTaskProcess(new Tasks.UploadBulkDocuments(_db, task.BulkDocument, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.UploadBulkDocuments))
            {
                Tasks.UploadBulkDocuments task = (Tasks.UploadBulkDocuments)sender;

                Commands.PostBulkDocumentsReply.Entry entry = task.FindEntryById(_version.VersionId.ToString());

                if (entry == null)
                {
                    TriggerOnError(task, "Failed to locate " + _version.VersionId.ToString() + " in the " +
                        "bulk document post results.", null);
                    return;
                }

                // This is needed so that couchdb can apply the content to the correct revision.
                _version.UpdateRevision(entry.Rev);

                // Upload Data.Content from Data.Version
                RunTaskProcess(new Tasks.UploadContent(_db, _version, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.UploadContent))
            {
                TriggerOnComplete(reply, new Tuple<Data.Resource, Data.Version>(_resource, _version));
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
