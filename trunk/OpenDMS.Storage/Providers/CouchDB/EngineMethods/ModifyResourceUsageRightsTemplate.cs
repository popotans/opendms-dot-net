using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    class ModifyResourceUsageRightsTemplate : Base
    {
        private List<Security.UsageRight> _usageRights;

        public ModifyResourceUsageRightsTemplate(EngineRequest request, List<Security.UsageRight> usageRights)
            : base(request)
        {
            _usageRights = usageRights;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.ModifyResourceUsageRightsTemplate process;

            process = new Transactions.Processes.ModifyResourceUsageRightsTemplate(_request.Database, _usageRights,
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
