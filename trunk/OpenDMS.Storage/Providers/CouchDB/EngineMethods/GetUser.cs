using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetUser : Base
    {
        private string _username = null;

        public GetUser(EngineRequest request, string username)
            : base(request)
        {
            _username = username;
        }

        public override void Execute()
        {
            GetGlobalPermissions();
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Not called
            throw new NotImplementedException();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            Commands.GetDocument cmd;
            Security.User user;

            if (!GetGlobalPermissions_OnComplete_IsAuthorized(request, reply, Security.Authorization.GlobalPermissionType.GetUser))
                return;

            user = new Security.User(_username);

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(request.Database, user));
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDocument command.", e);
                throw;
            }
            
            // Run it straight back to the subscriber
            AttachSubscriberEvent(cmd, _onProgress);
            AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.GettingUser, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            try
            {
                cmd.Execute(request.Database.Server.Timeout, request.Database.Server.Timeout, request.Database.Server.BufferSize, request.Database.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }
    }
}
