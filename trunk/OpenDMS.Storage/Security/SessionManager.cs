
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
        public delegate void AuthenticationDelegate(Session session, string message);
        public event ErrorDelegate OnError;
        public event InitializationCompletionDelegate OnInitializationComplete;
        public event AuthenticationDelegate OnAuthenticationComplete;

        private Providers.IEngine _engine;
        private Dictionary<Providers.IDatabase, DatabaseSessionManager> _dbSessionManagers;
        private int _dbsLeftToLoadGroups;

        public SessionManager()
        {
            _dbSessionManagers = new Dictionary<Providers.IDatabase, DatabaseSessionManager>();
        }

        public void Initialize(Providers.IEngine engine, List<Providers.IDatabase> databases)
        {
            _engine = engine;
            _dbsLeftToLoadGroups = databases.Count;

            try
            {
                for (int i = 0; i < databases.Count; i++)
                {
                    DatabaseSessionManager dsm = new DatabaseSessionManager(engine, databases[i]);
                    if (databases[i].SessionManager == null)
                        databases[i].SessionManager = dsm;
                    dsm.OnError += new DatabaseSessionManager.ErrorDelegate(Initialize_OnError);
                    dsm.OnLoadGroupsComplete += new DatabaseSessionManager.CompletionDelegate(Initialize_OnLoadGroupsComplete);
                    Logger.Storage.Debug("Loading the groups into the DatabaseSessionManager object...");
                    dsm.LoadGroups();
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Debug("An exception occurred while initializing the SessionManager.", e);
            }
        }

        private void Initialize_OnLoadGroupsComplete(DatabaseSessionManager sender, Providers.IDatabase db)
        {
            _dbsLeftToLoadGroups--;
            Logger.Storage.Debug("The DatabaseSesssionManager successfully loaded all groups.");
            _dbSessionManagers.Add(db, sender);
            if (_dbsLeftToLoadGroups <= 0)
            {
                _isInitialized = true;
                if (OnInitializationComplete != null) OnInitializationComplete();
            }
        }

        private void Initialize_OnError(string message, Exception exception)
        {
            Logger.Storage.Error("An error occurred while running DatabaseSessionManager.LoadGroups, message: " + message, exception);
            if (OnError != null) OnError(message, exception);
        }

        public void AuthenticateUser(Providers.IDatabase db, string username, string password)
        {
            CheckInitialization();

            if (!_dbSessionManagers.ContainsKey(db))
                throw new ArgumentException("Unable to locate sessions for the argument database.");

            DatabaseSessionManager dsm = _dbSessionManagers[db];
            dsm.OnError += new DatabaseSessionManager.ErrorDelegate(AuthenticateUser_OnError);
            dsm.OnAuthenticationComplete += new DatabaseSessionManager.AuthenticationDelegate(AuthenticateUser_OnAuthenticationComplete);
            Logger.Storage.Debug("Attempting to authenticate the user '" + username + "'...");
            Logger.Security.Debug("Authenticating user '" + username + "'...");
            dsm.AuthenticateUser(username, password);
        }

        private void AuthenticateUser_OnAuthenticationComplete(DatabaseSessionManager sender, Providers.IDatabase db, Session session, string message)
        {
            Logger.Storage.Debug("The DatabaseSesssionManager successfully authenticated the user.");
            if (session != null) Logger.Security.Debug("The user '" + session.User.Username + "' has been successfully authenticated and was given the AuthToken: " + session.AuthToken.ToString() + ".");
            else Logger.Security.Warn("The user failed authentication.");
            if (OnAuthenticationComplete != null) OnAuthenticationComplete(session, message);
        }

        private void AuthenticateUser_OnError(string message, Exception exception)
        {
            Logger.Storage.Error("An error occurred while running DatabaseSessionManager.AuthenticateUser, message: " + message, exception);
            Logger.Security.Error("The user could not be authenticated due to an unexpected error, see the storage log for additional information.");
            if (OnError != null) OnError(message, exception);
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
