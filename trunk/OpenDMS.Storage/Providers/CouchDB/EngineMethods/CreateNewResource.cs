using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateNewResource : Base
    {
        private IDatabase _db = null;
        private Data.Metadata _metadata = null;
        private Data.ResourceId _resourceId = null;
        private List<Security.UsageRight> _usageRights = null;

        public CreateNewResource(IDatabase db,
            Data.Metadata metadata,
            List<Security.UsageRight> usageRights,
            Engine.ActionDelegate onActionChanged,
            Engine.ProgressDelegate onProgress, 
            Engine.CompletionDelegate onComplete,
            Engine.TimeoutDelegate onTimeout, 
            Engine.ErrorDelegate onError)
            : base(onActionChanged, onProgress, onComplete, onTimeout, onError)
        {
            _db = db;
            _metadata = metadata;
            _usageRights = usageRights;
        }

        public override void Execute()
        {
            List<Exception> errors = null;
            Data.Resource resource;
            Model.Document doc;
            Commands.PutDocument cmd;

            if (_onActionChanged != null) _onActionChanged(EngineActionType.Preparing, false);

            _resourceId = Data.ResourceId.Create();
            resource = new Data.Resource(_resourceId, null, null, null, _metadata, _usageRights);

            doc = new Transitions.Resource().Transition(resource, out errors);

            if (errors != null && errors.Count > 0 && _onError != null)
            {
                for (int i = 0; i < errors.Count; i++)
                    _onError(errors[i].Message, errors[i]);
            }

            cmd = new Commands.PutDocument(_db, doc);

            if (_onActionChanged != null) _onActionChanged(EngineActionType.CreatingNewResource, false);

            AttachSubscriberEvent(cmd, _onProgress);
            AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);

            cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
        }
    }
}
