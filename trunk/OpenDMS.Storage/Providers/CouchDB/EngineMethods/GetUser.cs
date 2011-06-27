
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetUser : Base
    {
        private IDatabase _db = null;
        private string _username = null;

        public GetUser(EngineRequest request, IDatabase db, string username)
            : base(request)
        {
            _db = db;
            _username = username;
        }

        public override void Execute()
        {
            Commands.GetDocument cmd;
            Security.User user;

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, false);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            user = new Security.User(_username);

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(_db, user));
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
