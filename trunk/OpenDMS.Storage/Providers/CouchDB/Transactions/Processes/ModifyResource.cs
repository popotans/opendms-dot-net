using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class ModifyResource : Base
    {
        private IDatabase _db;
        private Data.Resource _resource;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private Data.Resource _storedResource;
        private JObject _storedRemainder;

        public ModifyResource(IDatabase db, Data.Resource resource,
            Security.RequestingPartyType requestingPartyType, Security.Session session)
        {
            _db = db;
            _resource = resource;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadResource(_db, _resource.ResourceId));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();
            // 1) Download
            // 2) Check Perms
            // 3) Upload Resource (no need to update versions, etc as we just want to change resource)

            if (t == typeof(Tasks.DownloadResource))
            {
                Tasks.DownloadResource task = (Tasks.DownloadResource)sender;
                _storedResource = task.Resource;
                _storedRemainder = task.Remainder;
                RunTaskProcess(new Tasks.CheckResourcePermissions(_db, _storedResource,
                    _requestingPartyType, _session, Security.Authorization.ResourcePermissionType.Modify));
            }
            else if (t == typeof(Tasks.CheckResourcePermissions))
            {
                Tasks.CheckResourcePermissions task = (Tasks.CheckResourcePermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                RunTaskProcess(new Tasks.UploadResource(_db, _resource));
            }
            else if (t == typeof(Tasks.UploadResource))
            {
                TriggerOnComplete(reply, _resource);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
