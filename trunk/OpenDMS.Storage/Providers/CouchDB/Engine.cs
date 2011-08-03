using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine : EngineBase
    {
		#region Fields (1) 

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

		#region Methods (19) 

		// Public Methods (17) 

        public override void AuthenticateUser(EngineRequest request, string username, string hashedPassword)
        {
            CheckInitialization();
            Logger.Storage.Debug("Authenticating user: " + username);
            EngineMethods.AuthenticateUser act = new EngineMethods.AuthenticateUser(request, _sessionMgr, username, hashedPassword);
            act.Execute();
        }

        public override void CreateGroup(EngineRequest request, Security.Group group)
        {
            CheckInitialization();
            Logger.Storage.Debug("Creating group: " + group.GroupName + "...");
            EngineMethods.CreateGroup act = new EngineMethods.CreateGroup(request, group);
            act.Execute();
        }

        public override void CreateNewResource(EngineRequest request, Data.Metadata resourceMetadata, Data.Metadata versionMetadata, Data.Content versionContent)
        {
            CheckInitialization();
            Logger.Storage.Debug("Creating new resource...");
            EngineMethods.CreateNewResource act = new EngineMethods.CreateNewResource(request, resourceMetadata, versionMetadata, versionContent);
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

        public override void Initialize(string transactionRootDirectory, string logDirectory,
            List<Providers.IDatabase> databases, InitializationDelegate onInitialized)
        {
            // We do not check initialization here
            new OpenDMS.IO.Logger(logDirectory);
            new OpenDMS.Networking.Logger(logDirectory);
            new OpenDMS.Storage.Logger(logDirectory);

            OpenDMS.IO.FileSystem.Instance.Initialize(8192);
            //Transactions.Manager.Instance.Initalize(new IO.Directory(transactionRootDirectory));

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

		#endregion Methods 
    }
}
