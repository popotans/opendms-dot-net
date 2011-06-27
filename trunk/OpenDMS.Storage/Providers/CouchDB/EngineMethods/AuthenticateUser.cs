using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class AuthenticateUser : Base
    {
        private EngineBase _engine;
        private Security.SessionManager _sessionMgr;
        private IDatabase _db = null;
        private string _username = null;
        private string _password = null;
        private bool _ignoringAuthenticationComplete;

        public AuthenticateUser(EngineBase engine, Security.SessionManager sessionMgr, IDatabase db, string username, string password)
            : base(null)
        {
            _engine = engine;
            _sessionMgr = sessionMgr;
            _db = db;
            _username = username;
            _password = password;
        }

        public override void Execute()
        {
            _sessionMgr.OnError += new Security.SessionManager.ErrorDelegate(AuthenticateUser_OnError);
            _sessionMgr.OnAuthenticationComplete += new Security.SessionManager.AuthenticationDelegate(AuthenticateUser_OnAuthenticationComplete);
            
            _ignoringAuthenticationComplete = false;

            try
            {
                _sessionMgr.AuthenticateUser(_db, _username, _password);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling SessionManager.AuthenticateUser.", e);
                throw;
            }
        }

        private void AuthenticateUser_OnAuthenticationComplete(Security.Session session, string message)
        {
            _sessionMgr.OnError -= AuthenticateUser_OnError;
            _sessionMgr.OnAuthenticationComplete -= AuthenticateUser_OnAuthenticationComplete;

            if (_ignoringAuthenticationComplete) return;

            try
            {
                _engine.TriggerOnAuthenticated(false, (session != null), session, message, null);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnAuthenticated event.", e);
                throw;
            }
        }

        private void AuthenticateUser_OnError(string message, Exception exception)
        {
            _ignoringAuthenticationComplete = true;

            _sessionMgr.OnError -= AuthenticateUser_OnError;
            _sessionMgr.OnAuthenticationComplete -= AuthenticateUser_OnAuthenticationComplete;

            try
            {
                _engine.TriggerOnAuthenticated(true, false, null, message, exception);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnAuthenticated event.", e);
                throw;
            }
        }
    }
}
