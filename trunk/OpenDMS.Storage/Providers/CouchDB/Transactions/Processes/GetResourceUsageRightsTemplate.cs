using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class GetResourceUsageRightsTemplate : Base
    {
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private GlobalUsageRights _gur;
        private ResourceUsageRightsTemplate _template;
        private ICommandReply _reply;

        public ResourceUsageRightsTemplate ResourceUsageRightsTemplate { get; private set; }

        public GetResourceUsageRightsTemplate(IDatabase db,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadResourceUsageRightsTemplate(_db, _sendTimeout, 
                _receiveTimeout, _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadResourceUsageRightsTemplate))
            {
                Tasks.DownloadResourceUsageRightsTemplate task = (Tasks.DownloadResourceUsageRightsTemplate)sender;
                _reply = reply;
                _template = task.Value;
                RunTaskProcess(new Tasks.DownloadGlobalPermissions(_db, _sendTimeout,
                    _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.DownloadGlobalPermissions))
            {
                Tasks.DownloadGlobalPermissions task = (Tasks.DownloadGlobalPermissions)sender;
                _gur = task.GlobalUsageRights;
                RunTaskProcess(new Tasks.CheckGlobalPermissions(_db, _gur, _requestingPartyType,
                    _session, Security.Authorization.GlobalPermissionType.GetResourceUsageRightsTemplate, 
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
                ResourceUsageRightsTemplate = _template;
                TriggerOnComplete(_reply, _gur);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
