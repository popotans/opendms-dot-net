using System;
using OpenDMS.Storage.Security;
using OpenDMS.Storage.Security.Authorization;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Database : IDatabase
    {
		#region Constructors (1) 

        public Database(IServer server, string dbName, DatabaseSessionManager sessionManager)
        {
            Server = server;
            Name = dbName;
            SessionManager = sessionManager;
        }

		#endregion Constructors 

		#region Properties (4) 

        public string Name { get; private set; }

        public IServer Server { get; private set; }

        public DatabaseSessionManager SessionManager { get; private set; }

        public Uri Uri
        {
            get { return new Uri(string.Format("{0}{1}/", Server.Uri, Name)); }
        }

		#endregion Properties 
    }
}