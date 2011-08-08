using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class RollbackResource : Base
    {
        private Data.ResourceId _resourceId;
        private int _rollbackDepth;

        public RollbackResource(EngineRequest request, Data.ResourceId resourceId, int rollbackDepth)
            : base(request)
        {
            _resourceId = resourceId;
            _rollbackDepth = rollbackDepth;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.RollbackResource process;

            process = new Transactions.Processes.RollbackResource(_request.Database, _resourceId, _rollbackDepth,
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
