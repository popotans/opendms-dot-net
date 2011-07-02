
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetAllGroups : Base
    {
        public GetAllGroups(EngineRequest request)
            : base(request)
        {
        }

        public override void Execute()
        {
            GetGlobalPermissions();
        }
        
        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            Commands.GetView cmd;

            if (!GetGlobalPermissions_OnComplete_IsAuthorized(request, reply, Security.Authorization.GlobalPermissionType.ListGroups))
                return;
            
            try
            {
                cmd = new Commands.GetView(UriBuilder.Build(request.Database, "groups", "GetAll"));
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetView command.", e);
                throw;
            }

            // Run it straight back to the subscriber
            AttachSubscriberEvent(cmd, _onProgress);
            AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);

            try
            {
                if (_onActionChanged != null) _onActionChanged(request, EngineActionType.GettingGroups, true);
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
