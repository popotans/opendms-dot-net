using System;
using Newtonsoft.Json.Linq;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public abstract class Save : Base
    {
        public Save(IDatabase db, string id, JObject input, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, id, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _input = input;
        }

        public abstract override void Process();
    }
}
