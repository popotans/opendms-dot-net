using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public class Attachment : BaseStorageObject
    {
        private bool? _stub = null;

        public bool? Stub
        {
            get { return _stub; }
            set { _stub = value; ResetLength(); }
        }

        public string ContentType
        {
            get { return this["content_type"].Value<string>(); }
            set { this["content_type"] = value; ResetLength(); }
        }

        public long AttachmentLength
        {
            get { return this["length"].Value<long>(); }
            set { this["length"] = value; ResetLength(); }
        }

        public Attachment()
            : base()
        {
            _stub = null;
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
            : base(jobj)
        {
        }

        private bool? GetStub()
        {
            if(this["stub"] != null)
                return this["stub"].Value<bool>();
            return null;
        }
    }
}