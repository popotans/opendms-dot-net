using Newtonsoft.Json.Linq;

namespace Common.Data.Providers.CouchDB.Model
{
    public class Attachment : JObject
    {
        public bool Stub
        {
            get { return this["stub"].Value<bool>(); }
            set { this["stub"] = value; }
        }

        public string ContentType
        {
            get { return this["content_type"].Value<string>(); }
            set { this["content_type"] = value; }
        }

        public long Length
        {
            get { return this["length"].Value<long>(); }
            set { this["length"] = value; }
        }

        public Attachment()
        {
        }

        public Attachment(string json)
            : this(Parse(json))
        {
        }

        public Attachment(JToken token)
            : this((JObject)token)
        {
        }

        public Attachment(JObject jobj)
            : this()
        {
        }
    }
}
