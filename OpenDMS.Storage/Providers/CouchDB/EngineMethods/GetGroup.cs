
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetGroup : Base
    {
        private string _groupName = null;

        public GetGroup(EngineRequest request, string groupName)
            : base(request)
        {
            _groupName = groupName;
        }

        public override void Execute()
        {
            GetGlobalPermissions();
        }

        protected override void  GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            Commands.GetDocument cmd;
            Security.Group group;

            group = new Security.Group(_groupName, null, null, null);

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(request.Database, group));
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
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Getting, true);
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
