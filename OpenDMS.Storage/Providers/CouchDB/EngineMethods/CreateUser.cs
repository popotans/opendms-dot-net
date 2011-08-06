using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateUser : Base
    {
        private Security.User _user;

        public CreateUser(EngineRequest request, Security.User user)
            : base(request)
        {
            _user = user;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.CreateUser process;

            process = new Transactions.Processes.CreateUser(_request.Database, _user,
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
