
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetAllGroupsForInitialization : Base
    {
        public GetAllGroupsForInitialization(EngineRequest request)
            : base(request)
        {
        }

        public override void Execute()
        {
            Commands.GetView cmd;

            try
            {
                cmd = new Commands.GetView(UriBuilder.Build(_request.Database, "groups", "GetAll"));
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
                cmd.Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Never called
            throw new System.NotImplementedException();
        }
    }
}
