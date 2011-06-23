using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetAllGroups : Base
    {
        private IDatabase _db = null;

        public GetAllGroups(IDatabase db,
            Engine.ActionDelegate onActionChanged,
            Engine.ProgressDelegate onProgress, 
            Engine.CompletionDelegate onComplete,
            Engine.TimeoutDelegate onTimeout, 
            Engine.ErrorDelegate onError)
            : base(onActionChanged, onProgress, onComplete, onTimeout, onError)
        {
            _db = db;
        }

        public override void Execute()
        {
            Commands.GetView cmd;

            if (_onActionChanged != null) _onActionChanged(EngineActionType.Preparing, false);

            cmd = new Commands.GetView(UriBuilder.Build(_db, "users", "GetAll"));
            
            // Run it straight back to the subscriber
            AttachSubscriberEvent(cmd, _onProgress);
            AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);

            if (_onActionChanged != null) _onActionChanged(EngineActionType.GettingGroups, true);

            cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
        }
    }
}
