using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class AuthenticateUser : Base
    {
        private IDatabase _db = null;
        private Security.SessionManager _sessionMgr;
        private string _username = null;
        private string _password = null;

        public bool IsAuthenticated { get; private set; }
        public Security.Session Session { get; private set; }

        public AuthenticateUser(IDatabase db, Security.SessionManager sessionMgr,
            string username, string encryptedPassword)
        {
            _sessionMgr = sessionMgr;
            _db = db;
            _username = username;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DownloadUser(_db, _username));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Tasks.DownloadUser task = (Tasks.DownloadUser)sender;
            Session = _sessionMgr.AuthenticateUser(_db, task.User, _password);
            IsAuthenticated = (Session != null);
            TriggerOnComplete(null, new Tuple<Security.Session, bool>(Session, IsAuthenticated));
        }
    }
}
