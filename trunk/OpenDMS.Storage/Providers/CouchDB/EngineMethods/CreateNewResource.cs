using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateNewResource : Base
    {
        private Data.Metadata _resourceMetadata = null;
        private Data.Metadata _versionMetadata = null;
        private Data.Content _versionContent = null;

        public CreateNewResource(EngineRequest request, Data.Metadata resourceMetadata, Data.Metadata versionMetadata, Data.Content versionContent)
            : base(request)
        {
            _resourceMetadata = resourceMetadata;
            _versionMetadata = versionMetadata;
            _versionContent = versionContent;
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.CreateNewResource process;

            process = new Transactions.Processes.CreateNewResource(_request.Database, _resourceMetadata, 
                _versionMetadata, _versionContent, _request.RequestingPartyType, _request.Session);
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
