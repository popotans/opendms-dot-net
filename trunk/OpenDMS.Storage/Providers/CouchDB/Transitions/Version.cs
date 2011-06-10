using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class Version
    {
        private Data.Version _version = null;
        private Model.Document _document = null;
        private List<Exception> _errors = null;

        public Version(Data.Version version)
        {
            _version = version;
            _document = new Model.Document();
            _errors = new List<Exception>();
        }

        public Model.Document Transition(out List<Exception> errors)
        {
            Model.Attachment att = null;

            _document.Id = _version.VersionId.ToString();
            _document.Rev = _version.Revision;

            if ((errors = AddMetadata(_version, _document)) != null)
                return null;

            if (_version.Content != null)
            {
                att = new Model.Attachment();
                att.ContentType = _version.Content.ContentType.Name;
                att.Length = _version.Content.Length;
                _document.AddAttachment(System.IO.Path.GetFileName(_version.Content.LocalFilepath), att);
            }

            return _document;
        }

        private List<Exception> AddMetadata(Data.Version version, Model.Document doc)
        {
            List<Exception> errors = null;
            Dictionary<string, object>.Enumerator en = version.Metadata.GetEnumerator();

            while (en.MoveNext())
            {
                if ((errors = AddMetadata(en.Current.Key, en.Current.Value, doc)) != null)
                    return errors;
            }

            return null;
        }

        public List<Exception> AddMetadata(string key, object value, Model.Document doc)
        {
            JObject jobj;
            JProperty jprop;
            string str = null;
            JsonSerializerSettings settings = new JsonSerializerSettings();

            settings.Error += delegate(object sender, ErrorEventArgs args)
            {
                if (args.CurrentObject == args.ErrorContext.OriginalObject)
                    _errors.Add(args.ErrorContext.Error);
            };

            //str = JsonConvert.SerializeObject(value);

            if (_errors.Count > 0)
            {
                // Reset the instance _errors so the user can retry
                List<Exception> retVal = _errors;
                _errors = new List<Exception>();
                return retVal;
            }

            //jobj = JObject.Parse(str);
            jprop = new JProperty(key, value);
            doc.Add(jprop);
            //doc.Add(key, (JToken)jobj);

            return null;
        }
    }
}
