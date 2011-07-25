using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks.Remoting
{
    public abstract class Base
    {
        private JObject _input;
        private JObject _output;
    }
}
