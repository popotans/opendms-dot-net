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

		#region Delegates and Events (8) 

		// Delegates (7) 

        public delegate void ActionDelegate(EngineRequest request, EngineActionType actionType, bool willSendProgress);
        public delegate void AuthorizationDelegate(EngineRequest request);
        public delegate void CompletionDelegate(EngineRequest request, ICommandReply reply, object result);
        public delegate void ErrorDelegate(EngineRequest request, string message, Exception exception);
        public delegate void InitializationDelegate(bool success, string message, Exception exception);
        public delegate void ProgressDelegate(EngineRequest request, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void TimeoutDelegate(EngineRequest request);
		// Events (1) 

        public event InitializationDelegate OnInitialized;

		#endregion Delegates and Events 

		#region Methods (30) 

		// Public Methods (30) 

        public virtual void AuthenticateUser(EngineRequest request, string username, string hashedPassword)
        {
            throw new NotImplementedException();
        }

        public virtual void CheckoutCurrentVersion(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void CheckoutResource(EngineRequest request, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public virtual void CheckoutVersion(EngineRequest request, Data.VersionId version)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateGroup(EngineRequest request, Security.Group group)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateNewResource(EngineRequest request, Data.Metadata resourceMetadata, Data.Metadata versionMetadata, Data.Content versionContent)
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

        public virtual void GetAllUsers(EngineRequest request)
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

        public void TriggerOnInitialized(bool success, string message, Exception exception)
        {
            if (OnInitialized != null) OnInitialized(success, message, exception);
        }

		#endregion Methods 
    }
}