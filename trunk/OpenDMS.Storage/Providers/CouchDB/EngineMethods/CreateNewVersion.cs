using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateNewVersion : Base
    {
        private IDatabase _db = null;
        private Data.Version _version = null;

        public CreateNewVersion(IDatabase db, 
            Data.Version version,
            Engine.ActionDelegate onActionChanged,
            Engine.ProgressDelegate onProgress, 
            Engine.CompletionDelegate onComplete,
            Engine.TimeoutDelegate onTimeout, 
            Engine.ErrorDelegate onError)
            : base(onActionChanged, onProgress, onComplete, onTimeout, onError)
        {
            _db = db;
            _version = version;
        }

        public override void Execute()
        {
            List<Exception> errors = null;
            Model.Document doc;
            Commands.PutDocument cmd;

            // State check
            if (!_version.CanCreateWithoutPropertiesOrContent &&
                !_version.CanCreateWithoutPropertiesWithContent &&
                !_version.CanCreateWithPropertiesAndContent &&
                !_version.CanCreateWithPropertiesWithoutContent)
                throw new ArgumentException("Argument version cannot be created due to its current state.");

            doc = new Transitions.Version().Transition(_version, out errors);

            if (errors != null && errors.Count > 0 && _onError != null)
            {
                for (int i = 0; i < errors.Count; i++)
                    _onError(errors[i].Message, errors[i]);
            }

            cmd = new Commands.PutDocument(_db, doc);


            cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
        }
    }
}
