using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class ModifyVersion : Base
    {
        private Data.Version _version;

        public ModifyVersion(EngineRequest request, Data.Version version)
            : base(request)
        {
            _version = version;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.ModifyVersion process;

            process = new Transactions.Processes.ModifyVersion(_request.Database, _version,
                _request.RequestingPartyType, _request.Session, _request.Database.Server.Timeout,
                _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
            t = new Transactions.Transaction(process);

            AttachSubscriber(process, _request.OnActionChanged);
            AttachSubscriber(process, _request.OnAuthorizationDenied);
            AttachSubscriber(process, _request.OnComplete);
            AttachSubscriber(process, _request.OnError);
            AttachSubscriber(process, _request.OnProgress);
            AttachSubscriber(process, _request.OnTimeout);

            t.Execute();
        }
    }
}
