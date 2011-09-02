using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class Version
    {

        private bool VerifyDocumentIntegrity(Model.Document document, out string property)
        {
            property = null;

            if (!CheckPropertyExistance(document, "_id", out property)) return false;
            if (!CheckPropertyExistance(document, "$type", out property)) return false;
            if (!CheckPropertyExistance(document, "$md5", out property)) return false;
            if (!CheckPropertyExistance(document, "$extension", out property)) return false;
            if (!CheckPropertyExistance(document, "$created", out property)) return false;
            if (!CheckPropertyExistance(document, "$creator", out property)) return false;
            if (!CheckPropertyExistance(document, "$modified", out property)) return false;
            if (!CheckPropertyExistance(document, "$modifier", out property)) return false;

            return true;
        }

        private bool CheckPropertyExistance(Model.Document document, string property, out string propertyName)
        {
            propertyName = null;

            if (document[property] == null)
            {
                propertyName = property;
                return false;
            }

            return true;
        }

        public Data.Version Transition(Model.Document document, out JObject remainder)
        {
            Data.Version version;
            Data.VersionId id;
            string rev;
            string verifyString;

            if (!VerifyDocumentIntegrity(document, out verifyString))
            {
                Logger.Storage.Error("The document is not properly formatted.  It is missing the property '" + verifyString + "'.");
                throw new FormattingException("The argument document does not have the necessary property '" + verifyString + "'.");
            }

            try
            {
                id = new Data.VersionId(document.Id);
                if (document["_rev"] != null)
                {
                    rev = document.Rev;
                    version = new Data.Version(id, rev);
                    document.Remove("_rev");
                }
                else
                {
                    version = new Data.Version(id);
                }

                document.Remove("_id");
                document.Remove("$type");

                version.Md5 = document["$md5"].Value<string>();
                version.Extension = document["$extension"].Value<string>();
                version.Created = document["$created"].Value<DateTime>();
                version.Creator = document["$creator"].Value<string>();
                version.Modified = document["$modified"].Value<DateTime>();
                version.Modifier = document["$modifier"].Value<string>();

                document.Remove("$md5");
                document.Remove("$extension");
                document.Remove("$created");
                document.Remove("$creator");
                document.Remove("$modified");
                document.Remove("$modifier");
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            remainder = document;
            return version;
        }

        public Model.Document Transition(Data.Version version, out List<Exception> errors)
        {
            Model.Document document = new Model.Document();
            Model.Attachment att = null;
            string prop;

            errors = null;

            try
            {
                document.Id = version.VersionId.ToString();
                if (!string.IsNullOrEmpty(version.Revision))
                    document.Rev = version.Revision;

                document.Add("$type", "version");
                document.Add("$md5", version.Md5);
                document.Add("$extension", version.Extension);
                document.Add("$created", version.Created);
                document.Add("$creator", version.Creator);
                document.Add("$modified", version.Modified);
                document.Add("$modifier", version.Modifier);

                if (version.Metadata != null)
                {
                    if ((errors = AddMetadata(version, document)) != null)
                        return null;
                }

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

                if (!VerifyDocumentIntegrity(document, out prop))
                {
                    Logger.Storage.Error("The document is not properly formatted.  It is missing the property '" + prop + "'.");
                    throw new FormattingException("The argument document does not have the necessary property '" + prop + "'.");
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

            if (key.StartsWith("$"))
            {
                errors.Add(new FormatException("Metadata keys cannot begin with '$'."));
                return errors;
            }

            jprop = new JProperty(key, value);
            doc.Add(jprop);

            return null;
        }
    }
}
