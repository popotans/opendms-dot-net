using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public class BulkDocuments : BaseStorageObject
    {
        private bool _documentsAreDirty = true;
        private List<Document> _documents = null;

        public bool AllOrNothing
        {
            get
            {
                if (this["all_or_nothing"] != null)
                    return this["all_or_nothing"].Value<bool>();
                else
                    return false;
            }
            set
            {
                this["all_or_nothing"] = value;
            }
        }

        public List<Document> Documents
        {
            get
            { 
                // If not dirty -> return them
                if (!_documentsAreDirty) return _documents;

                JArray jray = (JArray)this["docs"];

                // If null then return it
                if (jray == null) return (_documents = null);

                // Otherwise process documents
                for (int i = 0; i < jray.Count; i++)
                    _documents.Add((Document)jray[i]);

                _documentsAreDirty = false;
                return _documents;
            }
            set { _documents = value; ResetLength(); }
        }

        public bool HasDocument
        {
            get { return this["docs"] != null; }
        }

        public void AddDocument(Document document)
        {
            JArray jray = new JArray();
            _documents.Add(document);

            if (this["docs"] != null)
            {
                ((JArray)this["docs"]).Add(document);
            }
            else
            {
                jray.Add(document);
                Add("docs", jray);
            }
            ResetLength();
        }

        public BulkDocuments()
        {
            _documentsAreDirty = true;
            _documents = new List<Document>();
        }

        public BulkDocuments(string json)
            : this(Parse(json))
        {
        }

        public BulkDocuments(JToken token)
            : this((JObject)token)
        {
        }

        public BulkDocuments(JObject jobj)
            : base(jobj)
        {
        }
    }
}
