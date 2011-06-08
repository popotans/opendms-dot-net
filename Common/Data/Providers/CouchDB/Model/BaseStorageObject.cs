using Newtonsoft.Json.Linq;

namespace Common.Data.Providers.CouchDB.Model
{
    internal abstract class BaseStorageObject : JObject
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
