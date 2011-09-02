using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class Resource
    {
        public Resource()
        {
        }

        private bool VerifyDocumentIntegrity(Model.Document document, out string property)
        {
            property = null;

            if(!CheckPropertyExistance(document, "_id", out property)) return false;
            if(!CheckPropertyExistance(document, "$type", out property)) return false;
            if(!CheckPropertyExistance(document, "$versionids", out property)) return false;
            if(!CheckPropertyExistance(document, "$currentversionid", out property)) return false;
            if(!CheckPropertyExistance(document, "$usagerights", out property)) return false;
            if(!CheckPropertyExistance(document, "$tags", out property)) return false;
            if(!CheckPropertyExistance(document, "$created", out property)) return false;
            if(!CheckPropertyExistance(document, "$creator", out property)) return false;
            if(!CheckPropertyExistance(document, "$modified", out property)) return false;
            if(!CheckPropertyExistance(document, "$modifier", out property)) return false;
            if(!CheckPropertyExistance(document, "$checkedoutat", out property)) return false;
            if(!CheckPropertyExistance(document, "$checkedoutto", out property)) return false;
            if(!CheckPropertyExistance(document, "$lastcommit", out property)) return false;
            if(!CheckPropertyExistance(document, "$lastcommitter", out property)) return false;
            if(!CheckPropertyExistance(document, "$title", out property)) return false;

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

        public Data.Resource Transition(Model.Document document, out JObject remainder)
        {
            Data.Resource resource;
            Data.ResourceId id;
            string rev = null;
            Data.VersionId currentVersionId;
            List<Data.VersionId> versionIds = null;
            List<Security.UsageRight> usageRights = null;
            Security.UsageRight usageRight = null;
            JObject jobj = null;
            JProperty prop = null;
            IEnumerator<JToken> en;
            string verifyString;
            JArray jarray = new JArray();

            if (!VerifyDocumentIntegrity(document, out verifyString))
            {
                Logger.Storage.Error("The document is not properly formatted.  It is missing the property '" + verifyString + "'.");
                throw new FormattingException("The argument document does not have the necessary property '" + verifyString + "'.");
            }

            // I ran into a problem here where I was removing the properties from the document and what was left was the remainder.
            // However, this causes an issue when using the transition to make a resource for permissions checking as the
            // object returned to implementing software is the document.  Thus, the implementing software only received those properties
            // not removed... which obviously excludes the most important properties.  To remedy this issue, I created a constructor for
            // Model.Document(Document).  This constructor will format the argument document to a string and then create a JObject from 
            // that string.  C# will deep copy the string (not byref) so as to guarantee an independent object.
            remainder = new Model.Document(document);

            try
            {
                id = new Data.ResourceId(document.Id);
                if (document["_rev"] != null)
                {
                    rev = document.Rev;
                    remainder.Remove("_rev");
                }
                
                remainder.Remove("_id");
                remainder.Remove("$type");

                currentVersionId = new Data.VersionId(document["$currentversionid"].Value<string>());
                remainder.Remove("$currentversionid");

                versionIds = new List<Data.VersionId>();
                jarray = (JArray)document["$versionids"];

                for (int i = 0; i < jarray.Count; i++)
                    versionIds.Add(new Data.VersionId(jarray[i].Value<string>()));

                remainder.Remove("$versionids");


                usageRights = new List<Security.UsageRight>();
                jarray = (JArray)document["$usagerights"];

                for (int i = 0; i < jarray.Count; i++)
                {
                    jobj = (JObject)jarray[i];
                    en = jobj.Children().GetEnumerator();
                    while (en.MoveNext())
                    {
                        prop = (JProperty)en.Current;

                        // Json.Net is giving me some weird errors here when I try to call prop.value<int>();
                        // I cannot figure out why so this code is a temporary work-around, it needs figured out.
                        string a = prop.ToString();
                        a = a.Substring(a.LastIndexOf("\"") + 1); // we know the value is an int, so we can look for the last "
                        a = a.Replace(":", "").Trim();

                        usageRight = new Security.UsageRight(prop.Name, (Security.Authorization.ResourcePermissionType)int.Parse(a));
                        usageRights.Add(usageRight);
                    }
                }

                remainder.Remove("$usagerights");

                resource = new Data.Resource(id, rev, versionIds, currentVersionId, new Data.Metadata(), usageRights);

                // Tags
                resource.Tags = new List<string>();
                jarray = (JArray)document["$tags"];
                for (int i = 0; i < jarray.Count; i++)
                    resource.Tags.Add(jarray[i].Value<string>());
                remainder.Remove("$tags");

                resource.Created = document["$created"].Value<DateTime>();
                resource.Creator = document["$creator"].Value<string>();
                resource.Modified = document["$modified"].Value<DateTime>();
                resource.Modifier = document["$modifier"].Value<string>();
                resource.CheckedOutAt = document["$checkedoutat"].Value<DateTime>();
                resource.CheckedOutTo = document["$checkedoutto"].Value<string>();
                resource.LastCommit = document["$lastcommit"].Value<DateTime>();
                resource.LastCommitter = document["$lastcommitter"].Value<string>();
                resource.Title = document["$title"].Value<string>();

                remainder.Remove("$created");
                remainder.Remove("$creator");
                remainder.Remove("$modified");
                remainder.Remove("$modifier");
                remainder.Remove("$checkedoutat");
                remainder.Remove("$checkedoutto");
                remainder.Remove("$lastcommit");
                remainder.Remove("$lastcommitter");
                remainder.Remove("$title");
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            return resource;
        }

        public Model.Document Transition(Data.Resource resource, out List<Exception> errors)
        {
            Model.Document document = new Model.Document();
            JArray jarray;
            string prop;

            try
            {
                document.Id = resource.ResourceId.ToString();
                if (!string.IsNullOrEmpty(resource.Revision))
                    document.Rev = resource.Revision;
                document["$type"] = "resource";

                jarray = new JArray();
                for (int i = 0; i < resource.VersionIds.Count; i++)
                    jarray.Add(resource.VersionIds[i].ToString());
                document.Add("$versionids", jarray);


                jarray = new JArray();
                for (int i = 0; i < resource.UsageRights.Count; i++)
                {
                    JObject jobj = new JObject();
                    jobj.Add(new JProperty(resource.UsageRights[i].Entity, (int)resource.UsageRights[i].Permissions.Resource.Permissions));
                    jarray.Add(jobj);
                }
                document.Add("$usagerights", jarray);

                if (resource.CurrentVersionId != null)
                    document.Add("$currentversionid", resource.CurrentVersionId.ToString());


                jarray = new JArray();
                for (int i = 0; i < resource.Tags.Count; i++)
                    jarray.Add(resource.Tags[i]);
                document.Add("$tags", jarray);

                document.Add("$created", resource.Created);
                document.Add("$creator", resource.Creator);
                document.Add("$modified", resource.Modified);
                document.Add("$modifier", resource.Modifier);
                document.Add("$checkedoutat", resource.CheckedOutAt);
                document.Add("$checkedoutto", resource.CheckedOutTo);
                document.Add("$lastcommit", resource.LastCommit);
                document.Add("$lastcommitter", resource.LastCommitter);
                document.Add("$title", resource.Title);

                if ((errors = AddMetadata(resource, document)) != null)
                    return null;

                if(!VerifyDocumentIntegrity(document, out prop))
                {
                    Logger.Storage.Error("The document is not properly formatted.  It is missing the property '" + prop + "'.");
                    throw new FormattingException("The argument document does not have the necessary property '" + prop + "'.");
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the resource object.", e);
                throw;
            }

            return document;
        }

        public List<Exception> AddMetadata(Data.Resource resource, Model.Document doc)
        {
            if (resource.Metadata == null)
                return null;

            List<Exception> errors = null;
            Dictionary<string, object>.Enumerator en = resource.Metadata.GetEnumerator();

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
