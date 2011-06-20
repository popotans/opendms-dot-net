using System;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Database : IDatabase
    {
        public string Name { get; private set; }

        public IServer Server { get; private set; }

        public Uri Uri
        {
            get { return new Uri(string.Format("{0}{1}/", Server.Uri, Name)); }
        }

        public Database(Server server, string dbName)
        {
            Server = server;
            Name = dbName;
        }
    }
}