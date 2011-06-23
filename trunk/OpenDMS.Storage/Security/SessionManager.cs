
using System;
using System.Collections.Generic;
using OpenDMS.IO;

namespace OpenDMS.Storage.Security
{
    public class SessionManager : Singleton<SessionManager>
    {
        public delegate void ErrorDelegate(string message, Exception exception);
        public delegate void InitializationCompletionDelegate();
        public event ErrorDelegate OnError;
        public event InitializationCompletionDelegate OnInitializationComplete;

        private Dictionary<string, Group> _groups;
        private Dictionary<Guid, Session> _sessions;
        private Providers.IEngine _engine;

        public SessionManager()
        {
            _sessions = new Dictionary<Guid, Session>();
        }

        public void Initialize(Providers.IEngine engine)
        {
            LoadGroups();
        }

        private void LoadGroups()
        {
            Providers.EngineRequest request = new Providers.EngineRequest();
            request.OnComplete += new Providers.EngineBase.CompletionDelegate(LoadGroups_OnComplete);
            request.OnError += new Providers.EngineBase.ErrorDelegate(LoadGroups_OnError);
            request.OnTimeout += new Providers.EngineBase.TimeoutDelegate(LoadGroups_OnTimeout);
        }

        private void LoadGroups_OnComplete(Providers.ICommandReply reply)
        {
            Providers.CouchDB.Commands.GetViewReply cmdReply = (Providers.CouchDB.Commands.GetViewReply)reply;
            if (cmdReply.Ok)
            {
                if (OnInitializationComplete != null) OnInitializationComplete();
                _isInitialized = true;
            }
            else
            {
                if (OnError != null) OnError(cmdReply.ErrorMessage, null);
            }
        }

        private void LoadGroups_OnError(string message, Exception exception)
        {
            if (OnError != null) OnError(message, exception);
        }

        private void LoadGroups_OnTimeout()
        {
            if (OnError != null) OnError("A timeout occurred while attempting to load group information.", null);
        }

        //public Session AuthenticateUser(string username, string password)
        //{

        //}

        //public bool CheckAuthorization(Session session, UsageRight resourceUsageRights, UsageRight requestedUsageRights)
        //{
            
        //}
    }
}
