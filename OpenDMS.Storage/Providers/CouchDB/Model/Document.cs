using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public class Document : BaseStorageObject
    {
        private bool _attachmentsAreDirty = true;
        private Dictionary<string, Attachment> _attachments = null;

        public string Id
        {
            get { return this["_id"].Value<string>(); }
            set { this["_id"] = value; ResetLength(); }
        }

        public string Rev
        {
            get
            {
                JToken rev;
                if (this.TryGetValue("_rev", out rev))
                    return rev.Value<string>();
                return null;
            }
            set { this["_rev"] = value; ResetLength(); }
        }

        public Dictionary<string, Attachment> Attachments
        {
            get
            {
                // If not dirty -> return them
                if (!_attachmentsAreDirty)
                    return _attachments;
                else
                {
                    IEnumerator<JToken> en = null;
                    JToken attachmentsToken = this["_attachments"];

                    // if no additional attachments in document then return what is
                    // in _attachments
                    if (attachmentsToken == null)
                        return _attachments;

                    en = attachmentsToken.Children().GetEnumerator();
                    // Otherwise process attachments
                    while (en.MoveNext())
                    {
                        if (en.Current.Type != JTokenType.Property)
                            throw new Json.JsonParseException("Property was expected.");

                        _attachments.Add(((JProperty)en.Current).Name, (Attachment)en.Current);
                    }

                    _attachmentsAreDirty = false;
                }
                return _attachments;
            }
            set { _attachments = value; ResetLength(); }
        }

        public bool HasAttachment
        {
            get { return this["_attachments"] != null; }
        }

        public void AddAttachment(string name, Attachment attachment)
        {
            _attachments.Add(name, attachment);
            ResetLength();
        }

        public Document()
        {
            _attachmentsAreDirty = true;
            _attachments = new Dictionary<string, Attachment>();
        }

        public Document(string json)
            : this(Parse(json))
        {
        }

        public Document(JToken token)
            : this((JObject)token)
        {
        }

        public Document(JObject jobj)
            : base(jobj)
        {
        }

        public Document(Document doc)
            : base(doc.ToString())
        {            
        }

        public Document CombineWith(JObject jobj)
        {
            Newtonsoft.Json.JsonWriter jwriter = CreateWriter();
            IEnumerator<JToken> en = jobj.Children().GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current.Type != JTokenType.Property)
                    throw new Json.JsonParseException("Property was expected.");
                if (this[((JProperty)en.Current).Name] == null)
                {
                    Newtonsoft.Json.JsonConverter conv = new Newtonsoft.Json.Converters.KeyValuePairConverter();
                    en.Current.WriteTo(jwriter, new Newtonsoft.Json.JsonConverter[] { conv });
                }
            }

            return this;
        }
    }
}
