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
    }
}
