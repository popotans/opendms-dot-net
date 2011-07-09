using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers
{
    public class EngineBase : OpenDMS.IO.Singleton<EngineBase>, IEngine
    {
		#region Fields (2) 

        protected bool _isInitializing;
        protected Security.SessionManager _sessionMgr;

		#endregion Fields 

		#region Constructors (1) 

        public EngineBase()
        {
            _isInitialized = false;
            _sessionMgr = Security.SessionManager.Instance;
        }

		#endregion Constructors 

		#region Properties (1) 

        public bool IsInitializing { get { return _isInitializing; } }

		#endregion Properties 

		#region Delegates and Events (10) 

		// Delegates (8) 

        public delegate void ActionDelegate(EngineRequest request, EngineActionType actionType, bool willSendProgress);
        public delegate void AuthenticationDelegate(bool isError, bool isAuthenticated, Security.Session session, string message, Exception exception);
        public delegate void AuthorizationDelegate(EngineRequest request);
        public delegate void CompletionDelegate(EngineRequest request, ICommandReply reply);
        public delegate void ErrorDelegate(EngineRequest request, string message, Exception exception);
        public delegate void InitializationDelegate(bool success, string message, Exception exception);
        public delegate void ProgressDelegate(EngineRequest request, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void TimeoutDelegate(EngineRequest request);
		// Events (2) 

        public event AuthenticationDelegate OnAuthenticated;

        public event InitializationDelegate OnInitialized;

		#endregion Delegates and Events 

		#region Methods (34) 

		// Public Methods (34) 

        public virtual void AuthenticateUser(IDatabase db, string username, string hashedPassword, AuthenticationDelegate onAuthenticated)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateGroup(EngineRequest request, Security.Group group)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateNewResource(EngineRequest request, Data.Metadata metadata)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateNewResource(EngineRequest request, Data.Metadata metadata, List<Security.UsageRight> usageRights)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateNewVersion(EngineRequest request, Data.Version version)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateUser(EngineRequest request, Security.User user)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteResource(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void DetermineIfInstalled(EngineRequest request, string logDirectory)
        {
            throw new NotImplementedException();
        }

        public virtual void GetAllGroups(EngineRequest request)
        {
            throw new NotImplementedException();
        }

        public virtual void GetAllGroupsForInitialization(EngineRequest request)
        {
            throw new NotImplementedException();
        }

        public virtual void GetAllUsers(EngineRequest request)
        {
            throw new NotImplementedException();
        }

        public virtual void GetCurrentVersion(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void GetGlobalPermissions(EngineRequest request)
        {
            throw new NotImplementedException();
        }

        public virtual void GetGroup(EngineRequest request, string groupName)
        {
            throw new NotImplementedException();
        }

        public virtual void GetResource(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void GetResourcePermissions(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void GetResourceReadOnly(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void GetResourceUsageRightsTemplate(EngineRequest request)
        {
            throw new NotImplementedException();
        }

        public virtual void GetUser(EngineRequest request, string username)
        {
            throw new NotImplementedException();
        }

        public virtual void GetVersion(EngineRequest request, Data.VersionId version)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize(string transactionRootDirectory, string logDirectory,
            List<Providers.IDatabase> databases, InitializationDelegate onInitialized)
        {
            _isInitialized = true;
        }

        public virtual void Install(EngineRequest request, string logDirectory)
        {
            throw new NotImplementedException();
        }

        public virtual void ModifyGlobalPermissions(EngineRequest request, List<Security.UsageRight> usageRights)
        {
            throw new NotImplementedException();
        }

        public virtual void ModifyGroup(EngineRequest request, Security.Group group)
        {
            throw new NotImplementedException();
        }

        public virtual void ModifyResource(EngineRequest request, Data.Resource resource)
        {
            throw new NotImplementedException();
        }

        public virtual void ModifyResourceUsageRightsTemplate(EngineRequest request, List<Security.UsageRight> usageRights)
        {
            throw new NotImplementedException();
        }

        public virtual void ModifyUser(EngineRequest request, Security.User user)
        {
            throw new NotImplementedException();
        }

        public virtual void ModifyVersion(EngineRequest request, Data.Version version)
        {
            throw new NotImplementedException();
        }

        public void RegisterOnAuthenticated(AuthenticationDelegate onAuthenticated)
        {
            OnAuthenticated += onAuthenticated;
        }

        public void RegisterOnInitialized(InitializationDelegate onInitialized)
        {
            OnInitialized += onInitialized;
        }

        public virtual void RollbackResource(EngineRequest request, Data.ResourceId resource, int rollbackDepth)
        {
            throw new NotImplementedException();
        }

        public virtual void SetState(bool isInitializing, bool isInitialized)
        {
            _isInitialized = isInitialized;
            _isInitializing = isInitializing;
        }

        public void TriggerOnAuthenticated(bool isError, bool isAuthenticated, Security.Session session, string message, Exception exception)
        {
            if (OnAuthenticated != null) OnAuthenticated(isError, isAuthenticated, session, message, exception);
        }

        public void TriggerOnInitialized(bool success, string message, Exception exception)
        {
            if (OnInitialized != null) OnInitialized(success, message, exception);
        }

		#endregion Methods 
    }
}
