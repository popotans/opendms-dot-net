using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class AuthenticateUser : Base
    {
		#region Fields (4) 

        private IDatabase _db = null;
        private string _password = null;
        private Security.SessionManager _sessionMgr;
        private string _username = null;

		#endregion Fields 

		#region Constructors (1) 

        public AuthenticateUser(EngineRequest request, Security.SessionManager sessionMgr, string username, string password)
            : base(request)
        {
            _request = request;
            _sessionMgr = sessionMgr;
            _db = request.Database;
            _username = username;
            _password = password;
        }

		#endregion Constructors 

		#region Methods (1) 

		// Public Methods (1) 

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Processes.AuthenticateUser process;

            process = new Transactions.Processes.AuthenticateUser(_db, _sessionMgr, _username, _password);
            t = new Transactions.Transaction(process);

            AttachSubscriber(process, _request.OnActionChanged);
            AttachSubscriber(process, _request.OnAuthorizationDenied);
            AttachSubscriber(process, _request.OnComplete);
            AttachSubscriber(process, _request.OnError);
            AttachSubscriber(process, _request.OnProgress);
            AttachSubscriber(process, _request.OnTimeout);

            t.Execute();
        }

		#endregion Methods 
    }
}
