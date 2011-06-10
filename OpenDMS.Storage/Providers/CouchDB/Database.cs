using System;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Database : Model.BaseStorageObject
    {
        /// <summary>
        /// The Database name
        /// </summary>
        private string _name;
        /// <summary>
        /// The Database Server
        /// </summary>
        private Server _server;

        public Uri Uri
        {
            get { return new Uri(string.Format("{0}{1}", _server.Uri, _name)); }
        }

        public Database(Server server, string dbName)
        {
            _server = server;
            _name = dbName;
        }
    }
}