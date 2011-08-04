using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class CreateUser : Base
    {
        private Security.User _user;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private GlobalUsageRights _gur;

        public Security.User User { get; private set; }

        public CreateUser(IDatabase db, Security.User user,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _user = user;
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
                    _session, Security.Authorization.GlobalPermissionType.CreateUser, _sendTimeout, _receiveTimeout,
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
                RunTaskProcess(new Tasks.UploadUser(_db, _user, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            }
            else if (t == typeof(Tasks.UploadUser))
            {
                TriggerOnComplete(reply, _user);
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
