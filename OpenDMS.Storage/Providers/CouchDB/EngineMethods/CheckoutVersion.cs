using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CheckoutVersion : Base
    {
        private Data.VersionId _versionId;

        public CheckoutVersion(EngineRequest request, Data.VersionId versionId)
            : base(request)
        {
            _versionId = versionId;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.CheckoutVersion process;

            process = new Transactions.Processes.CheckoutVersion(_request.Database, _versionId,
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
