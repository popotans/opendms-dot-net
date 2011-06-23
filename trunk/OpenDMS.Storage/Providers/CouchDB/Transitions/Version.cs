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
            List<Security.UsageRight> usageRights = null;
            Security.UsageRight usageRight = null;
            JProperty prop = null;

            id = new Data.VersionId(document.Id);
            rev = document.Rev;
            document["Type"] = "version";

            if (document["UsageRights"] != null)
            {
                usageRights = new List<Security.UsageRight>();
                JArray jarray = (JArray)document["UsageRights"];

                for (int i = 0; i < jarray.Count; i++)
                {
                    prop = (JProperty)jarray[i];
                    usageRight = new Security.UsageRight(prop.Name, (Security.PermissionType)prop.Value<int>());
                    usageRights.Add(usageRight);
                }

                document.Remove("UsageRights");
            }

            document.Remove("_id");
            if (document["_rev"] != null)
                document.Remove("_rev");
            document.Remove("Type");

            remainder = document;
            return new Data.Version(id, rev, usageRights);
        }

        public Model.Document Transition(Data.Version version, out List<Exception> errors)
        {
            Model.Document doc = null;
            Model.Attachment att = null;
            JArray jarray = null;

            doc.Id = version.VersionId.ToString();
            if (!string.IsNullOrEmpty(version.Revision))
                doc.Rev = version.Revision;

            if ((errors = AddMetadata(version, doc)) != null)
                return null;

            if (version.Content != null)
            {
                att = new Model.Attachment();
                att.ContentType = version.Content.ContentType.Name;
                att.AttachmentLength = version.Content.Length;
                doc.AddAttachment(System.IO.Path.GetFileName(version.Content.LocalFilepath), att);
            }

            if (version.UsageRights != null && version.UsageRights.Count > 0)
            {
                jarray = new JArray();

                for (int i = 0; i < version.UsageRights.Count; i++)
                    jarray.Add(new JProperty(version.UsageRights[i].Entity, (int)version.UsageRights[i].Permissions));

                doc.Add("UsageRights", jarray);
            }

            return doc;
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
