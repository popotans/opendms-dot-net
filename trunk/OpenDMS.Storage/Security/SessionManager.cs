
using System;
using System.Collections.Generic;
using OpenDMS.IO;
using OpenDMS.Storage.Providers.CouchDB.Commands;

namespace OpenDMS.Storage.Security
{
    public class SessionManager : Singleton<SessionManager>
    {
        public delegate void ErrorDelegate(string message, Exception exception);
        public delegate void InitializationCompletionDelegate();
        public event ErrorDelegate OnError;
        public event InitializationCompletionDelegate OnInitializationComplete;

        private Providers.IEngine _engine;
        private Dictionary<Providers.IDatabase, DatabaseSessionManager> _dbSessionManagers;
        private int _dbsLeftToLoadGroups;

        public SessionManager()
        {
            _dbSessionManagers = new Dictionary<Providers.IDatabase, DatabaseSessionManager>();
        }

        public void Initialize(Providers.IEngine engine, Dictionary<Providers.IDatabase, DatabaseSessionManager> dsms)
        {
            _engine = engine;
            _dbSessionManagers = dsms;
        }

        public Session AuthenticateUser(Providers.IDatabase db, User user, string passwordToTest)
        {
            CheckInitialization();

            if (!_dbSessionManagers.ContainsKey(db))
                throw new ArgumentException("Unable to locate sessions for the argument database.");

            DatabaseSessionManager dsm = _dbSessionManagers[db];
            Logger.Storage.Debug("Attempting to authenticate the user '" + user.Username + "'...");
            Logger.Security.Debug("Authenticating user '" + user.Username + "'...");
            return dsm.AuthenticateUser(user, passwordToTest);
        }

        public Session LookupSession(Providers.IDatabase db, Guid authToken)
        {
            Session session;

            Logger.Security.Debug("Running session lookup for authentication token '" + authToken.ToString() + "'...");

            if (!_dbSessionManagers.ContainsKey(db))
            {
                Logger.Security.Error("The database '" + db.Name + "' could not be found in the collection of managed databases.");
                throw new UnknownDatabaseException("The database could not be found.");
            }

            session = _dbSessionManagers[db].LookupSession(authToken);

            if (session == null)
                Logger.Security.Debug("A session could not be found for authentication token " + authToken.ToString());
            else
                Logger.Security.Debug("A session was found for user '" + session.User.Username + "'");

            return session;
        }

        public DatabaseSessionManager LookupDatabaseSessionManager(Providers.IDatabase db)
        {
            if (_dbSessionManagers.ContainsKey(db))
                return _dbSessionManagers[db];
            return null;
        }
    }
}
