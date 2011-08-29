using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class ModifyGlobalPermissions : Base
    {
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private List<Security.UsageRight> _usageRights;
        private GlobalUsageRights _gur;

        public GlobalUsageRights GlobalUsageRights { get; private set; }

        public ModifyGlobalPermissions(IDatabase db, List<Security.UsageRight> usageRights, 
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
                if (_usageRights[i].Permissions.Resource != null)
                {
                    throw new ArgumentException("Resource usage rights are not proper within the global usage rights.");
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
                    _session, Security.Authorization.GlobalPermissionType.ModifyGlobalPermissions,
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
                _gur = new CouchDB.GlobalUsageRights(_gur.Revision, _usageRights);
                RunTaskProcess(new Tasks.UploadGlobalUsageRights(_db, _gur, _sendTimeout,
                    _receiveTimeout, _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.UploadGlobalUsageRights))
            {
                Tasks.UploadGlobalUsageRights task = (Tasks.UploadGlobalUsageRights)sender;
                GlobalUsageRights = task.GlobalUsageRights;
                TriggerOnComplete(reply, _gur);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
