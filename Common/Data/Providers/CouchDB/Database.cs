using System;
using Common.Http.Methods;
using Common.Data.Providers.CouchDB.Commands;

namespace Common.Data.Providers.CouchDB
{
    public class Database : BaseStorageObject
    {
        /// <summary>
        /// The Database name
        /// </summary>
        private string _name;
        /// <summary>
        /// The Database Server
        /// </summary>
        private Server _server;

        public override Uri Uri
        {
            get { return new Uri(string.Format("{0}{1}", _server.Uri, _name)); }
        }

        public override List List
        {
            get {   return new Commands.List(
                        new Http.Methods.HttpGet(
                            new Uri(string.Format("{0}{1}", _server.Uri, "_all_dbs"))
                        )
                    );
            }
        }

        public override Update Update
        {
            get { throw new UnsupportedException("CouchDB does not support updating of a database"); }
        }
    } 
}