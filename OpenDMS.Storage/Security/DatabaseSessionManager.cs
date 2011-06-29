using System;
using System.Collections.Generic;
using OpenDMS.Storage.Providers.CouchDB.Commands;

namespace OpenDMS.Storage.Security
{
    public class DatabaseSessionManager
    {
        private const int SESSION_DURATION = 15; // Minutes

        public delegate void CompletionDelegate(DatabaseSessionManager sender, Providers.IDatabase db);
        public delegate void ErrorDelegate(string message, Exception exception);
        public delegate void AuthenticationDelegate(DatabaseSessionManager sender, Providers.IDatabase db, Session session, string message);
        public event CompletionDelegate OnLoadGroupsComplete;
        public event ErrorDelegate OnError;
        public event AuthenticationDelegate OnAuthenticationComplete;

        private Providers.IEngine _engine;
        private Providers.IDatabase _db;
        private Dictionary<Guid, Session> _sessions;
        private List<Group> _groups;
        private bool _ignoringLoadGroupComplete;

        public DatabaseSessionManager(Providers.IEngine engine, Providers.IDatabase db)
        {
            _engine = engine;
            _db = db;
            _sessions = new Dictionary<Guid, Session>();
            _ignoringLoadGroupComplete = false;
        }

        public void LoadGroups()
        {
            Providers.EngineRequest request = new Providers.EngineRequest();
            request.OnComplete += new Providers.EngineBase.CompletionDelegate(LoadGroups_OnComplete);
            request.OnError += new Providers.EngineBase.ErrorDelegate(LoadGroups_OnError);
            request.OnTimeout += new Providers.EngineBase.TimeoutDelegate(LoadGroups_OnTimeout);
            request.RequestingPartyType = RequestingPartyType.System;

            _ignoringLoadGroupComplete = false;

            Logger.Storage.Debug("Asking the engine to load all groups for the database named " + _db.Name + ".");
            _engine.GetAllGroups(request, _db);
        }

        private void LoadGroups_OnComplete(Providers.EngineRequest request, Providers.ICommandReply reply)
        {
            request.OnComplete -= LoadGroups_OnComplete;
            request.OnError -= LoadGroups_OnError;
            request.OnTimeout -= LoadGroups_OnTimeout;

            if (_ignoringLoadGroupComplete) return;

            try
            {
                GetViewReply cmdReply = (GetViewReply)reply;
                Providers.CouchDB.Transitions.GroupCollection gc;

                if (cmdReply.Ok)
                {
                    gc = new Providers.CouchDB.Transitions.GroupCollection();
                    _groups = gc.Transition(cmdReply.View);
                    Logger.Storage.Debug("All groups for the database named " + _db.Name + " have been loaded.  There were " + _groups.Count.ToString() + " group(s) located.");
                    if (OnLoadGroupsComplete != null) OnLoadGroupsComplete(this, _db);
                }
                else
                {
                    if (OnError != null) OnError(cmdReply.ErrorMessage, null);
                }
            }
            catch (Exception e)
            {
                if (OnError != null) OnError(e.Message, e);
            }
        }

        private void LoadGroups_OnError(Providers.EngineRequest request, string message, Exception exception)
        {
            request.OnComplete -= LoadGroups_OnComplete;
            request.OnError -= LoadGroups_OnError;
            request.OnTimeout -= LoadGroups_OnTimeout;
            Logger.Storage.Error("An error occurred while running GetAllGroups, message: " + message, exception);
            if (OnError != null) OnError(message, exception);
        }

        private void LoadGroups_OnTimeout(Providers.EngineRequest request)
        {
            // Completion could happen after a timeout, if we are going to error on a timeout, we need
            // to ignore the following completion
            _ignoringLoadGroupComplete = true;

            request.OnComplete -= LoadGroups_OnComplete;
            request.OnError -= LoadGroups_OnError;
            request.OnTimeout -= LoadGroups_OnTimeout;

            Logger.Storage.Error("A timeout occurred while running GetAllGroups.");
            if (OnError != null) OnError("A timeout occurred while attempting to load group information.", null);
        }

        public void AuthenticateUser(string username, string password)
        {
            Providers.EngineRequest request = new Providers.EngineRequest();
            request.OnComplete += new Providers.EngineBase.CompletionDelegate(AuthenticateUser_OnComplete);
            request.OnError += new Providers.EngineBase.ErrorDelegate(AuthenticateUser_OnError);
            request.OnTimeout += new Providers.EngineBase.TimeoutDelegate(AuthenticateUser_OnTimeout);
            request.RequestingPartyType = RequestingPartyType.System;
            request.UserToken = password;

            Logger.Storage.Debug("Asking the engine to get the information for a the specific user '" + username + "'.");
            _engine.GetUser(request, _db, username);
        }

        private void AuthenticateUser_OnComplete(Providers.EngineRequest request, Providers.ICommandReply reply)
        {
            GetDocumentReply cmdReply = (GetDocumentReply)reply;
            Providers.CouchDB.Transitions.User txUser;
            Security.User user;

            if (cmdReply.Ok)
            {
                txUser = new Providers.CouchDB.Transitions.User();
                user = txUser.Transition(cmdReply.Document);
                if (user.Password == (string)request.UserToken)
                {
                    Session session = new Session(user, Guid.NewGuid(), DateTime.Now.AddMinutes(SESSION_DURATION));
                    _sessions.Add(session.AuthToken, session);
                    Logger.Storage.Debug("User '" + session.User.Username + "' successfully authenticated and given the authentication token: " + session.AuthToken);
                    if (OnAuthenticationComplete != null) OnAuthenticationComplete(this, _db, session, "User authenticated successfully.");
                }
                else
                {
                    Logger.Storage.Warn("User '" + user + "' failed authentication due to an incorrect password.");
                    if (OnAuthenticationComplete != null) OnAuthenticationComplete(this, _db, null, "Authentication failed, invalid username and/or password.");
                }
            }
            else
            {
                Logger.Storage.Warn("The requested user was not found.");
                if (OnAuthenticationComplete != null) OnAuthenticationComplete(this, _db, null, "Authentication failed, invalid username and/or password.");
            }
        }

        private void AuthenticateUser_OnError(Providers.EngineRequest request, string message, Exception exception)
        {
            Logger.Storage.Error("An error occurred while running GetUser, message: " + message, exception);
            if (OnError != null) OnError(message, exception);
        }

        private void AuthenticateUser_OnTimeout(Providers.EngineRequest request)
        {
            Logger.Storage.Error("A timeout occurred while running GetUser.");
            if (OnError != null) OnError("A timeout occurred while attempting to download user information.", null);
        }

        public Session LookupSession(Guid authToken)
        {
            if (!_sessions.ContainsKey(authToken))
                return null;

            return _sessions[authToken];
        }
    }
}
