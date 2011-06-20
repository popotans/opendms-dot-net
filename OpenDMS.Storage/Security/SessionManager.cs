
using System;
using System.Collections.Generic;
using OpenDMS.IO;

namespace OpenDMS.Storage.Security
{
    public class SessionManager : Singleton<SessionManager>
    {
        private Dictionary<string, Group> _groups;
        private Dictionary<Guid, Session> _sessions;
        private Providers.IEngine _engine;

        public SessionManager()
        {
            _sessions = new Dictionary<Guid, Session>();
        }

        public void Initialize(Providers.IEngine engine)
        {
            _isInitialized = true;
        }

        private void LoadGroups()
        {
        }

        public Session AuthenticateUser(string username, string password)
        {

        }

        public bool CheckAuthorization(Session session, UsageRight resourceUsageRights, UsageRight requestedUsageRights)
        {
            
        }
    }
}
