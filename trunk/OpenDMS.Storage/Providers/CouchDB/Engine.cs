using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine : EngineBase
    {
        private bool _ignoringInitializationComplete;
        private bool _ignoringAuthenticateComplete;
        private bool _isInitializing;

        public Engine()
            : base()
        {
            Logger.Storage.Debug("Instantiating engine...");
            _ignoringInitializationComplete = false;
            _ignoringAuthenticateComplete = false;
            _isInitializing = false;
            Logger.Storage.Debug("Engine instantiated.");
        }

        public override void Initialize(List<Providers.IDatabase> databases, InitializationDelegate onInitialized)
        {
            Logger.Storage.Debug("Initializing engine...");
            _sessionMgr.OnError += new Security.SessionManager.ErrorDelegate(Initialize_OnError);
            _sessionMgr.OnInitializationComplete += new Security.SessionManager.InitializationCompletionDelegate(Initialize_OnInitializationComplete);
            RegisterOnInitialized(onInitialized);
            _ignoringInitializationComplete = false;
            _isInitializing = true;
            _sessionMgr.Initialize(this, databases);
        }

        private void Initialize_OnInitializationComplete()
        {
            _sessionMgr.OnError -= Initialize_OnError;
            _sessionMgr.OnInitializationComplete -= Initialize_OnInitializationComplete;

            if (_ignoringInitializationComplete) return;

            _isInitialized = true;
            _isInitializing = false;
            Logger.Storage.Debug("Engine initialized.");
            TriggerOnInitialized(true, "Initialization successful.", null);
        }

        private void Initialize_OnError(string message, Exception exception)
        {
            _ignoringInitializationComplete = true;

            _sessionMgr.OnError -= Initialize_OnError;
            _sessionMgr.OnInitializationComplete -= Initialize_OnInitializationComplete;

            _isInitialized = false;
            _isInitializing = false;
            Logger.Storage.Error("An error occurred while trying to initialize the engine.", exception);
            TriggerOnInitialized(false, message, exception);
        }

        public override void AuthenticateUser(IDatabase db, string username, string hashedPassword, AuthenticationDelegate onAuthenticated)
        {
            CheckInitialization();
            Logger.Storage.Debug("Authenticating user: " + username);
            //_sessionMgr.OnError += new Security.SessionManager.ErrorDelegate(AuthenticateUser_OnError);
            //_sessionMgr.OnAuthenticationComplete += new Security.SessionManager.AuthenticationDelegate(AuthenticateUser_OnAuthenticationComplete);
            RegisterOnAuthenticated(onAuthenticated);
            EngineMethods.AuthenticateUser act = new EngineMethods.AuthenticateUser(this, _sessionMgr, db, username, hashedPassword);
            act.Execute();
        }

        private void AuthenticateUser_OnAuthenticationComplete(Security.Session session, string message)
        {
            _sessionMgr.OnError -= AuthenticateUser_OnError;
            _sessionMgr.OnAuthenticationComplete -= AuthenticateUser_OnAuthenticationComplete;

            if (_ignoringAuthenticateComplete) return;

            if (session != null)
                Logger.Storage.Debug("User: " + session.User.Username + " was authenticated and given the token: " + session.AuthToken.ToString());
            else
                Logger.Storage.Debug("User failed authentication.");

            TriggerOnAuthenticated(false, (session != null), session, message, null);
        }

        private void AuthenticateUser_OnError(string message, Exception exception)
        {
            _ignoringAuthenticateComplete = true;

            _sessionMgr.OnError -= AuthenticateUser_OnError;
            _sessionMgr.OnAuthenticationComplete -= AuthenticateUser_OnAuthenticationComplete;

            Logger.Storage.Error("An error occurred while trying to authenticate the user.", exception);
            TriggerOnAuthenticated(true, false, null, message, exception);
        }

        public override void GetAllGroups(EngineRequest request, IDatabase db)
        {
            // SessionManager needs access to this method, so this method can be used when initializing.
            // Alternatively this could be built into SessionManager, but for now, we will just do this.
            if (!_isInitialized && !_isInitializing)
                throw new OpenDMS.IO.NotInitializedException();

            Logger.Storage.Debug("Getting all groups from db: " + db.Name + " on server: " + db.Server.Uri.ToString());
            EngineMethods.GetAllGroups act = new EngineMethods.GetAllGroups(request, db);
            act.Execute();
        }

        public override void GetGroup(EngineRequest request, IDatabase db, string groupName)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting group: " + groupName + " from db: " + db.Name + " on server: " + db.Server.Uri.ToString());
            EngineMethods.GetGroup act = new EngineMethods.GetGroup(request, db, groupName);
            act.Execute();
        }

        public override void GetUser(EngineRequest request, IDatabase db, string username)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting user: " + username + " from db: " + db.Name + " on server: " + db.Server.Uri.ToString());
            EngineMethods.GetUser act = new EngineMethods.GetUser(request, db, username);
            act.Execute();
        }

        public override void CreateUser(EngineRequest request, IDatabase db, Security.User user)
        {
            CheckInitialization();
            Logger.Storage.Debug("Creating user: " + user.Username + " in db: " + db.Name + " on server: " + db.Server.Uri.ToString());



            //EngineMethods.c
        }

    }
}
