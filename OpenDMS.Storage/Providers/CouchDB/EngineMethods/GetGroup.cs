
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetGroup : Base
    {
        private IDatabase _db = null;
        private string _groupName = null;

        public GetGroup(IDatabase db,
            string groupName,
            Engine.ActionDelegate onActionChanged,
            Engine.ProgressDelegate onProgress, 
            Engine.CompletionDelegate onComplete,
            Engine.TimeoutDelegate onTimeout, 
            Engine.ErrorDelegate onError)
            : base(onActionChanged, onProgress, onComplete, onTimeout, onError)
        {
            _db = db;
            _groupName = groupName;
        }

        public override void Execute()
        {
            Commands.GetDocument cmd;
            Security.Group group;

            if (_onActionChanged != null) _onActionChanged(EngineActionType.Preparing, false);

            group = new Security.Group(_groupName, null, null, null);
            cmd = new Commands.GetDocument(UriBuilder.Build(_db, group));
            
            // Run it straight back to the subscriber
            AttachSubscriberEvent(cmd, _onProgress);
            AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);

            if (_onActionChanged != null) _onActionChanged(EngineActionType.Getting, true);

            cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
        }
    }
}
