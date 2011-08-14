using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class ModifyResource : Base
    {
        private Data.Resource _resource;

        public ModifyResource(EngineRequest request, Data.Resource resource)
            : base(request)
        {
            _resource = resource;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.ModifyResource process;

            process = new Transactions.Processes.ModifyResource(_request.Database, _resource,
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
