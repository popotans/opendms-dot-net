using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class GetAllGroups : Base
    {
        private IDatabase _db;
        private GlobalUsageRights _gur;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;

        public List<Security.Group> Groups { get; private set; }

        public GetAllGroups(IDatabase db, Security.RequestingPartyType requestingPartyType, 
            Security.Session session)
        {
            _db = db;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadGlobalPermissions(_db));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {   
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadGlobalPermissions))
            {
                Tasks.DownloadGlobalPermissions task = (Tasks.DownloadGlobalPermissions)sender;
                _gur = task.GlobalUsageRights;
                RunTaskProcess(new Tasks.CheckGlobalPermissions(_db, _gur, _requestingPartyType,
                    _session, Security.Authorization.GlobalPermissionType.CreateResource));
            }
            else if (t == typeof(Tasks.CheckGlobalPermissions))
            {
                Tasks.CheckGlobalPermissions task = (Tasks.CheckGlobalPermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }
                RunTaskProcess(new Tasks.DownloadGroups(_db));
            }
            else if (t == typeof(Tasks.DownloadGroups))
            {
                Tasks.DownloadGroups task = (Tasks.DownloadGroups)sender;
                Groups = task.Groups;
                TriggerOnComplete(reply, Groups);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
