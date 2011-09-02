using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class CreateNewResource : Base
    {
        //private Data.Metadata _resourceMetadata = null;
        //private Data.Metadata _versionMetadata = null;
        //private Data.Content _versionContent = null;
        private CreateResourceArgs _args;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private GlobalUsageRights _gur;
        private Data.Resource _resource;
        private Data.Version _version;

        public Model.BulkDocuments BulkDocuments { get; private set; }

        public CreateNewResource(IDatabase db, CreateResourceArgs args,
            Security.RequestingPartyType requestingPartyType,
            Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _args = args;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadGlobalPermissions(_db, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadGlobalPermissions))
            {
                Tasks.DownloadGlobalPermissions task = (Tasks.DownloadGlobalPermissions)sender;
                _gur = task.GlobalUsageRights;
                RunTaskProcess(new Tasks.CheckGlobalPermissions(_db, _gur, _requestingPartyType,
                    _session, Security.Authorization.GlobalPermissionType.CreateResource, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.CheckGlobalPermissions))
            {
                Tasks.CheckGlobalPermissions task = (Tasks.CheckGlobalPermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                RunTaskProcess(new Tasks.DownloadResourceUsageRightsTemplate(_db, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.DownloadResourceUsageRightsTemplate))
            {
                string username = _session.User.Username;
                DateTime creation = DateTime.Now;
                List<Exception> errors;
                List<Model.Document> docs = new List<Model.Document>();

                Tasks.DownloadResourceUsageRightsTemplate task = (Tasks.DownloadResourceUsageRightsTemplate)sender;
                
                // Create the Resource and Version objects
                List<Data.VersionId> versions = new List<Data.VersionId>();

                Data.ResourceId resourceId = Data.ResourceId.Create();

                _version = new Data.Version(new Data.VersionId(resourceId), _args.VersionArgs.Metadata, _args.VersionArgs.Content)
                {
                    Md5 = _args.VersionArgs.Md5,
                    Extension = _args.VersionArgs.Extension,
                    Created = creation,
                    Creator = username,
                    Modified = creation,
                    Modifier = username
                };

                versions.Add(_version.VersionId);

                _resource = new Data.Resource(resourceId, null, versions, _version.VersionId,
                    _args.Metadata, task.Value.UsageRights)
                {
                    Tags = _args.Tags,
                    Created = creation,
                    Creator = username,
                    Modified = creation,
                    Modifier = username,
                    CheckedOutAt = creation,
                    CheckedOutTo = username,
                    LastCommit = creation,
                    LastCommitter = username,
                    Title = _args.Title
                };
                
                // Transition to json objects
                
                Transitions.Resource txResource = new Transitions.Resource();
                docs.Add(txResource.Transition(_resource, out errors));
                if (errors != null)
                {
                    TriggerOnError(null, errors[0].Message, errors[0]);
                    return;
                }
                Transitions.Version txVersion = new Transitions.Version();
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
