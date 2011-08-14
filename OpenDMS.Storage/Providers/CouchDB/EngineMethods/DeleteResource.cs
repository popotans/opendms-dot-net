using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class DeleteResource : Base
    {
        private Data.ResourceId _resourceId;

        public DeleteResource(EngineRequest request, Data.ResourceId resourceId)
            : base(request)
        {
            _resourceId = resourceId;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.DeleteResource process;

            process = new Transactions.Processes.DeleteResource(_request.Database, _resourceId,
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
