using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public abstract class BaseStorageObject : JObject
    {
        public BaseStorageObject()
        {
        }

        public BaseStorageObject(string json)
            : this(Parse(json))
        {
        }

        public BaseStorageObject(JToken token)
            : this((JObject)token)
        {
        }

        public BaseStorageObject(JObject jobj)
            : this()
        {
        }
    }
}