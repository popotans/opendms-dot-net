using System;
using System.Collections.Generic;
using OpenDMS.Storage.Providers.CouchDB.Commands;

namespace OpenDMS.Storage.Security
{
    public class DatabaseSessionManager
    {
        private const int SESSION_DURATION = 15; // Minutes

        private Providers.IEngine _engine;
        private Providers.IDatabase _db;
        private Dictionary<Guid, Session> _sessions;
        private List<Group> _groups;

        public DatabaseSessionManager(Providers.IEngine engine, Providers.IDatabase db)
        {
            _engine = engine;
            _db = db;
            _db.SessionManager = this;
            _sessions = new Dictionary<Guid, Session>();
        }

        public DatabaseSessionManager(Providers.IEngine engine, Providers.IDatabase db, List<Security.Group> groups)
        {
            _engine = engine;
            _db = db;
            _db.SessionManager = this;
            _sessions = new Dictionary<Guid, Session>();
            _groups = groups;
        }

        public Session AuthenticateUser(User user, string passwordToTest)
        {
            if (user.Password == passwordToTest)
            {
                Session session = new Session(user, Guid.NewGuid(), DateTime.Now.AddMinutes(SESSION_DURATION));
                _sessions.Add(session.AuthToken, session);
                Logger.Storage.Debug("User '" + session.User.Username + "' successfully authenticated and given the authentication token: " + session.AuthToken);
                return session;
            }
            else
            {
                Logger.Storage.Warn("User '" + user + "' failed authentication due to an incorrect password.");
                return null;
            }
        }

        public Session LookupSession(Guid authToken)
        {
            if (!_sessions.ContainsKey(authToken))
                return null;

            return _sessions[authToken];
        }

        public List<Group> GroupMembershipOfUser(string username)
        {
            List<Group> groups = new List<Group>();

            if (username.StartsWith("user-"))
                username = username.Substring(5);

            for (int i = 0; i < _groups.Count; i++)
                if (_groups[i].UserIsMember(username))
                    groups.Add(_groups[i]);

            return groups;
        }
    }
}
