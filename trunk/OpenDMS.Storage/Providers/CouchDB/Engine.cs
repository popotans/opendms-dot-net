using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine : EngineBase
    {
		#region Fields (3) 

        private bool _ignoringAuthenticateComplete;

		#endregion Fields 

		#region Constructors (1) 

        public Engine()
            : base()
        {
            if (Logger.Storage != null) Logger.Storage.Debug("Instantiating engine...");
            _ignoringAuthenticateComplete = false;
            _isInitializing = false;
            if (Logger.Storage != null) Logger.Storage.Debug("Engine instantiated.");
        }

		#endregion Constructors 

		#region Methods (12) 

        // Public Methods (8) 
        public override void ModifyUser(EngineRequest request, Security.User user)
        {
            CheckInitialization();
            Logger.Storage.Debug("Modifying user: " + user.Username + "...");
            EngineMethods.ModifyUser act = new EngineMethods.ModifyUser(request, user);
            act.Execute();
        }

        public override void ModifyGroup(EngineRequest request, Security.Group group)
        {
            CheckInitialization();
            Logger.Storage.Debug("Modifying group: " + group.GroupName + "...");
            EngineMethods.ModifyGroup act = new EngineMethods.ModifyGroup(request, group);
            act.Execute();
        }

        public override void CreateGroup(EngineRequest request, Security.Group group)
        {
            CheckInitialization();
            Logger.Storage.Debug("Creating group: " + group.GroupName + "...");
            EngineMethods.CreateGroup act = new EngineMethods.CreateGroup(request, group);
            act.Execute();
        }

        public override void DetermineIfInstalled(EngineRequest request, string logDirectory)
        {
            new OpenDMS.IO.Logger(logDirectory);
            new OpenDMS.Networking.Logger(logDirectory);
            new OpenDMS.Storage.Logger(logDirectory);
            Logger.Storage.Debug("Checking if OpenDMS.Storage has been installed on the db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.DetermineIfInstalled act = new EngineMethods.DetermineIfInstalled(request);
            act.Execute();
        }

        public override void AuthenticateUser(IDatabase db, string username, string hashedPassword, AuthenticationDelegate onAuthenticated)
        {
            CheckInitialization();
            Logger.Storage.Debug("Authenticating user: " + username);
            RegisterOnAuthenticated(onAuthenticated);
            EngineMethods.AuthenticateUser act = new EngineMethods.AuthenticateUser(this, _sessionMgr, db, username, hashedPassword);
            act.Execute();
        }

        public override void CreateUser(EngineRequest request, Security.User user)
        {
            CheckInitialization();
            Logger.Storage.Debug("Creating user: " + user.Username + " in db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.CreateUser act = new EngineMethods.CreateUser(request, user);
            act.Execute();
        }

        public override void GetAllGroups(EngineRequest request)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting all groups from db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.GetAllGroups act = new EngineMethods.GetAllGroups(request);
            act.Execute();
        }

        public override void GetAllUsers(EngineRequest request)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting all users from db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.GetAllUsers act = new EngineMethods.GetAllUsers(request);
            act.Execute();
        }

        public override void GetAllGroupsForInitialization(EngineRequest request)
        {
            // Doing it for initialization, don't check initialization but do check state
            if (!_isInitializing)
                throw new InvalidOperationException("The engine is not initializing.");
            else if (_isInitialized)
                throw new InvalidOperationException("The engine is already initialized.");

            EngineMethods.GetAllGroupsForInitialization act = new EngineMethods.GetAllGroupsForInitialization(request);
            act.Execute();
        }

        public override void GetGlobalPermissions(EngineRequest request)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting global permissions in db: " + request.Database.Name);
            EngineMethods.GetGlobalPermissions act = new EngineMethods.GetGlobalPermissions(request);
            act.Execute();
        }

        public override void GetGroup(EngineRequest request, string groupName)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting group: " + groupName + " from db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.GetGroup act = new EngineMethods.GetGroup(request, groupName);
            act.Execute();
        }

        public override void GetUser(EngineRequest request, string username)
        {
            CheckInitialization();
            Logger.Storage.Debug("Getting user: " + username + " from db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.GetUser act = new EngineMethods.GetUser(request, username);
            act.Execute();
        }

        public override void Initialize(string transactionRootDirectory, string logDirectory,
            List<Providers.IDatabase> databases, InitializationDelegate onInitialized)
        {
            // We do not check initialization here
            new OpenDMS.IO.Logger(logDirectory);
            new OpenDMS.Networking.Logger(logDirectory);
            new OpenDMS.Storage.Logger(logDirectory);

            OpenDMS.IO.FileSystem.Instance.Initialize(8192);
            Transactions.Manager.Instance.Initalize(new IO.Directory(transactionRootDirectory));

            EngineRequest request = new EngineRequest();
            request.Engine = this;
            request.RequestingPartyType = Security.RequestingPartyType.System;
            EngineMethods.Initialize act = new EngineMethods.Initialize(request, _sessionMgr, 
                onInitialized, databases);
            act.Execute();
        }

        public override void Install(EngineRequest request, string logDirectory)
        {
            // Do not check initialization as it should not be initialized
            // CheckInitialization();
            if (_isInitialized) throw new InvalidOperationException("Install cannot be run on an initialized database.");

            new OpenDMS.IO.Logger(logDirectory);
            new OpenDMS.Networking.Logger(logDirectory);
            new OpenDMS.Storage.Logger(logDirectory);

            Logger.Storage.Debug("Installing to db: " + request.Database.Name + " on server: " + request.Database.Server.Uri.ToString());
            EngineMethods.Install act = new EngineMethods.Install(request);
            act.Execute();
        }
		// Private Methods (4) 

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

		#endregion Methods 
    }
}
