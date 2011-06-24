using System;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers
{
    public class EngineBase : OpenDMS.IO.Singleton<EngineBase>, IEngine
    {
        public delegate void ActionDelegate(EngineActionType actionType, bool willSendProgress);
        public delegate void ProgressDelegate(DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void CompletionDelegate(ICommandReply reply);
        public delegate void TimeoutDelegate();
        public delegate void ErrorDelegate(string message, Exception exception);

        protected Security.SessionManager _sessionMgr;

        public EngineBase()
        {
            _isInitialized = false;
            _sessionMgr = Security.SessionManager.Instance;
        }

        public virtual void Initialize()
        {
            _isInitialized = true;
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
    }
}
