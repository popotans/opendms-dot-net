using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers
{
    public class EngineBase : OpenDMS.IO.Singleton<EngineBase>, IEngine
    {
        public delegate void InitializationDelegate(bool success, string message, Exception exception);
        public delegate void AuthenticationDelegate(bool isError, bool isAuthenticated, Security.Session session, string message, Exception exception);
        public event InitializationDelegate OnInitialized;
        public event AuthenticationDelegate OnAuthenticated;

        public delegate void ActionDelegate(EngineRequest request, EngineActionType actionType, bool willSendProgress);
        public delegate void ProgressDelegate(EngineRequest request, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void CompletionDelegate(EngineRequest request, ICommandReply reply);
        public delegate void TimeoutDelegate(EngineRequest request);
        public delegate void ErrorDelegate(EngineRequest request, string message, Exception exception);

        protected Security.SessionManager _sessionMgr;

        public EngineBase()
        {
            _isInitialized = false;
            _sessionMgr = Security.SessionManager.Instance;
        }

        public virtual void Initialize(List<Providers.IDatabase> databases, InitializationDelegate onInitialized)
        {
            _isInitialized = true;
        }

        public void RegisterOnInitialized(InitializationDelegate onInitialized)
        {
            OnInitialized += onInitialized;
        }

        public void TriggerOnInitialized(bool success, string message, Exception exception)
        {
            if (OnInitialized != null) OnInitialized(success, message, exception);
        }

        public virtual void AuthenticateUser(IDatabase db, string username, string hashedPassword, AuthenticationDelegate onAuthenticated)
        {
            throw new NotImplementedException();
        }

        public void RegisterOnAuthenticated(AuthenticationDelegate onAuthenticated)
        {
            OnAuthenticated += onAuthenticated;
        }

        public void TriggerOnAuthenticated(bool isError, bool isAuthenticated, Security.Session session, string message, Exception exception)
        {
            if (OnAuthenticated != null) OnAuthenticated(isError, isAuthenticated, session, message, exception);
        }

        public virtual void GetAllGroups(EngineRequest request, IDatabase db)
        {
            throw new NotImplementedException();
        }
        
        public virtual void GetGroup(EngineRequest request, IDatabase db, string groupName)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateNewResource(EngineRequest request, IDatabase db, Data.Metadata metadata, System.Collections.Generic.List<Security.UsageRight> usageRights)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateNewVersion(EngineRequest request, IDatabase db, Data.Version version)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateGroup(EngineRequest request, IDatabase db, Security.Group group)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateGroup(EngineRequest request, IDatabase db, Security.Group group)
        {
            throw new NotImplementedException();
        }

        public virtual void GetAllUsers(EngineRequest request, IDatabase db)
        {
            throw new NotImplementedException();
        }

        public virtual void GetUser(EngineRequest request, IDatabase db, string username)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateUser(EngineRequest request, IDatabase db, Security.User user)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateUser(EngineRequest request, IDatabase db, Security.User user)
        {
            throw new NotImplementedException();
        }


        public void GetResource(EngineRequest request, IDatabase db, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public void GetResourceReadOnly(EngineRequest request, IDatabase db, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public void CreateNewResource(EngineRequest request, IDatabase db, Data.Metadata metadata)
        {
            throw new NotImplementedException();
        }

        public void ModifyResource(EngineRequest request, IDatabase db, Data.Resource resource)
        {
            throw new NotImplementedException();
        }

        public void RollbackResource(EngineRequest request, IDatabase db, Data.ResourceId resource, int rollbackDepth)
        {
            throw new NotImplementedException();
        }

        public void DeleteResource(EngineRequest request, IDatabase db, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public void GetVersion(EngineRequest request, IDatabase db, Data.VersionId version)
        {
            throw new NotImplementedException();
        }

        public void GetCurrentVersion(EngineRequest request, IDatabase db, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public void UpdateVersion(EngineRequest request, IDatabase db, Data.Version version)
        {
            throw new NotImplementedException();
        }

        public void ModifyVersion(EngineRequest request, IDatabase db, Data.Version version)
        {
            throw new NotImplementedException();
        }

        public void GetResourcePermissions(EngineRequest request, IDatabase db, Data.ResourceId resource)
        {
            throw new NotImplementedException();
        }

        public void GetGlobalPermissions(EngineRequest request, IDatabase db)
        {
            throw new NotImplementedException();
        }

        public void UpdateGlobalPermissions(EngineRequest request, IDatabase db, List<Security.UsageRight> usageRights)
        {
            throw new NotImplementedException();
        }

        public void GetResourceUsageRightsTemplate(EngineRequest request, IDatabase db)
        {
            throw new NotImplementedException();
        }

        public void ModifyResourceUsageRightsTemplate(EngineRequest request, IDatabase db, List<Security.UsageRight> usageRights)
        {
            throw new NotImplementedException();
        }
    }
}
