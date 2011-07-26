using System;
using Newtonsoft.Json.Linq;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks.Remoting
{
    public class Save : Base
    {
        public Save(IDatabase db, string id, JObject input)
            : base(db, id)
        {
            _input = input;
        }

        public virtual override void Process()
        {
            throw new NotImplementedException();
        }
    }
}
