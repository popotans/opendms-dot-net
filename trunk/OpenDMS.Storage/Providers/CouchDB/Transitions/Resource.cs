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

        public Data.Resource Transition(Model.Document document, out JObject remainder)
        {
            Data.ResourceId id;
            string rev, checkedOutTo = null;
            DateTime? checkedOutAt = null;
            Data.VersionId currentVersionId;
            List<Data.VersionId> versionIds = null;
            List<Security.UsageRight> usageRights = null;
            Security.UsageRight usageRight = null;
            JObject jobj = null;
            JProperty prop = null;
            IEnumerator<JToken> en;

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
                rev = document.Rev;

                if (document["CheckedOutTo"] != null)
                {
                    checkedOutTo = document["CheckedOutTo"].Value<string>();
                    remainder.Remove("CheckedOutTo");
                }
                if (document["CheckedOutAt"] != null)
                {
                    checkedOutAt = document["CheckedOutAt"].Value<DateTime?>();
                    remainder.Remove("CheckedOutAt");
                }
                
                remainder.Remove("_id");
                remainder.Remove("_rev");
                remainder.Remove("Type");

                if (document["CurrentVersionId"] != null)
                {
                    currentVersionId = new Data.VersionId(document["CurrentVersionId"].Value<string>());
                    remainder.Remove("CurrentVersionId");
                }
                else
                    currentVersionId = null;

                if (document["VersionIds"] != null)
                {
                    versionIds = new List<Data.VersionId>();
                    JArray jarray = (JArray)document["VersionIds"];

                    for (int i = 0; i < jarray.Count; i++)
                        versionIds.Add(new Data.VersionId(jarray[i].Value<string>()));

                    remainder.Remove("VersionIds");
                }

                if (document["UsageRights"] != null)
                {
                    usageRights = new List<Security.UsageRight>();
                    JArray jarray = (JArray)document["UsageRights"];

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

                    remainder.Remove("UsageRights");
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            return new Data.Resource(id, rev, checkedOutTo, checkedOutAt, versionIds, currentVersionId, new Data.Metadata(), usageRights);
        }

        public Model.Document Transition(Data.Resource resource, out List<Exception> errors)
        {
            Model.Document document = new Model.Document();
            JArray jarray;

            try
            {
                document.Id = resource.ResourceId.ToString();
                if (!string.IsNullOrEmpty(resource.CheckedOutTo))
                    document["CheckedOutTo"] = resource.CheckedOutTo;
                if (resource.CheckedOutAt.HasValue)
                    document["CheckedOutAt"] = resource.CheckedOutAt.Value;

                if (!string.IsNullOrEmpty(resource.Revision))
                    document.Rev = resource.Revision;

                document["Type"] = "resource";

                if (resource.CurrentVersionId != null)
                    document.Add("CurrentVersionId", resource.CurrentVersionId.ToString());

                if (resource.VersionIds != null && resource.VersionIds.Count > 0)
                {
                    jarray = new JArray();

                    for (int i = 0; i < resource.VersionIds.Count; i++)
                        jarray.Add(resource.VersionIds[i].ToString());

                    document.Add("VersionIds", jarray);
                }

                if ((errors = AddMetadata(resource, document)) != null)
                    return null;

                if (resource.UsageRights != null && resource.UsageRights.Count > 0)
                {
                    jarray = new JArray();

                    for (int i = 0; i < resource.UsageRights.Count; i++)
                    {
                        JObject jobj = new JObject();
                        jobj.Add(new JProperty(resource.UsageRights[i].Entity, (int)resource.UsageRights[i].Permissions.Resource.Permissions));
                        jarray.Add(jobj);
                    }

                    document.Add("UsageRights", jarray);
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

            jprop = new JProperty(key, value);
            doc.Add(jprop);

            return null;
        }
    }
}
