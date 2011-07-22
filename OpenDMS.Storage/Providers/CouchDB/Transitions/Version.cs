using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class Version
    {
        public Data.Version Transition(Model.Document document, out JObject remainder)
        {
            Data.VersionId id;
            string rev;

            try
            {
                id = new Data.VersionId(document.Id);
                rev = document.Rev;
                document["Type"] = "version";

                document.Remove("_id");
                if (document["_rev"] != null)
                    document.Remove("_rev");
                document.Remove("Type");
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            remainder = document;
            return new Data.Version(id, rev);
        }

        public Model.Document Transition(Data.Version version, out List<Exception> errors)
        {
            Model.Document document = new Model.Document();
            Model.Attachment att = null;

            try
            {
                document.Id = version.VersionId.ToString();
                if (!string.IsNullOrEmpty(version.Revision))
                    document.Rev = version.Revision;

                if ((errors = AddMetadata(version, document)) != null)
                    return null;

                if (version.Content != null)
                {
                    att = new Model.Attachment();
                    att.ContentType = version.Content.ContentType.Name;
                    att.AttachmentLength = version.Content.Length;
                    if (string.IsNullOrEmpty(version.Content.LocalFilepath))
                        document.AddAttachment("file", att);
                    else
                        document.AddAttachment(System.IO.Path.GetFileName(version.Content.LocalFilepath), att);
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the version object.", e);
                throw;
            }

            return document;
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
            JProperty jprop;
            JsonSerializerSettings settings = new JsonSerializerSettings();
            List<Exception> errors = new List<Exception>();

            settings.Error += delegate(object sender, ErrorEventArgs args)
            {
                if (args.CurrentObject == args.ErrorContext.OriginalObject)
                    errors.Add(args.ErrorContext.Error);
            };

            if (errors.Count > 0)
                return errors;

            jprop = new JProperty(key, value);
            doc.Add(jprop);

            return null;
        }
    }
}
