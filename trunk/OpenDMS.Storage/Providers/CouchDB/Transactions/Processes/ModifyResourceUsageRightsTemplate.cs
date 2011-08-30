using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class ModifyResourceUsageRightsTemplate : Base
    {
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private List<Security.UsageRight> _usageRights;
        private GlobalUsageRights _gur;
        private ResourceUsageRightsTemplate _rurt;

        public ResourceUsageRightsTemplate ResourceUsageRightsTemplate { get; private set; }

        public ModifyResourceUsageRightsTemplate(IDatabase db, List<Security.UsageRight> usageRights,
            Security.RequestingPartyType requestingPartyType, 
            Security.Session session, int sendTimeout, int receiveTimeout, int sendBufferSize, 
            int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _usageRights = usageRights;
            _requestingPartyType = requestingPartyType;
            _session = session;

            for (int i = 0; i < _usageRights.Count; i++)
            {
                // Throw an exception in the event that a resource usage right is included.
                if (_usageRights[i].Permissions.Global != null)
                {
                    throw new ArgumentException("Global usage rights are not proper within the resource usage rights template.");
                }
            }
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadGlobalPermissions(_db, _sendTimeout, 
                _receiveTimeout, _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DownloadGlobalPermissions))
            {
                Tasks.DownloadGlobalPermissions task = (Tasks.DownloadGlobalPermissions)sender;
                _gur = task.GlobalUsageRights;
                RunTaskProcess(new Tasks.CheckGlobalPermissions(_db, _gur, _requestingPartyType,
                    _session, Security.Authorization.GlobalPermissionType.ModifyResourceUsageRightsTemplate,
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
                RunTaskProcess(new Tasks.DownloadResourceUsageRightsTemplate(_db, _sendTimeout, 
                    _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.DownloadResourceUsageRightsTemplate))
            {
                Tasks.DownloadResourceUsageRightsTemplate task = (Tasks.DownloadResourceUsageRightsTemplate)sender;
                _rurt = new CouchDB.ResourceUsageRightsTemplate(task.Value.Revision, _usageRights);
                RunTaskProcess(new Tasks.UploadResourceUsageRightsTemplate(_db, _rurt,
                    _sendTimeout, _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.UploadResourceUsageRightsTemplate))
            {
                Tasks.UploadResourceUsageRightsTemplate task = (Tasks.UploadResourceUsageRightsTemplate)sender;
                _rurt = task.Value;
                ResourceUsageRightsTemplate = _rurt;
                TriggerOnComplete(reply, ResourceUsageRightsTemplate);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
