using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetGroup : Base
    {
        private Security.Group _group;

        public GetGroup(EngineRequest request, Security.Group group)
            : base(request)
        {
            _group = group;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.GetGroup process;

            process = new Transactions.Processes.GetGroup(_request.Database, _group,
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
