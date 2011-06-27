
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetGroup : Base
    {
        private IDatabase _db = null;
        private string _groupName = null;

        public GetGroup(EngineRequest request, IDatabase db, string groupName)
            : base(request)
        {
            _db = db;
            _groupName = groupName;
        }

        public override void Execute()
        {
            Commands.GetDocument cmd;
            Security.Group group;

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, false);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            group = new Security.Group(_groupName, null, null, null);

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(_db, group));
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
                cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }
    }
}
