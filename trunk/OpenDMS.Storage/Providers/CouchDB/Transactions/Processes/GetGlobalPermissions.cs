using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class GetGlobalPermissions : Base
    {
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private GlobalUsageRights _gur;

        public GlobalUsageRights GlobalUsageRights { get; private set; }

        public GetGlobalPermissions(IDatabase db, Security.RequestingPartyType requestingPartyType, 
            Security.Session session, int sendTimeout, int receiveTimeout, int sendBufferSize, 
            int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _requestingPartyType = requestingPartyType;
            _session = session;
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
                    _session, Security.Authorization.GlobalPermissionType.GetGlobalPermissions, 
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
                GlobalUsageRights = _gur;
                TriggerOnComplete(reply, _gur);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
