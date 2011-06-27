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
            string rev;
            Data.VersionId currentVersionId;
            List<Data.VersionId> versionIds = null;
            List<Security.UsageRight> usageRights = null;
            Security.UsageRight usageRight = null;
            JProperty prop = null;

            try
            {
                id = new Data.ResourceId(document.Id);
                rev = document.Rev;
                if (document["CurrentVersionId"] != null)
                {
                    currentVersionId = new Data.VersionId(document["CurrentVersionId"].Value<string>());
                    document.Remove("CurrentVersionId");
                }
                else
                    currentVersionId = null;

                if (document["VersionIds"] != null)
                {
                    versionIds = new List<Data.VersionId>();
                    JArray jarray = (JArray)document["VersionIds"];

                    for (int i = 0; i < jarray.Count; i++)
                        versionIds.Add(new Data.VersionId(jarray[i].Value<string>()));

                    document.Remove("VersionIds");
                }

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

                // Strip out the stuff we handle
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
            return new Data.Resource(id, rev, versionIds, currentVersionId, null, usageRights);
        }

        public Model.Document Transition(Data.Resource resource, out List<Exception> errors)
        {
            Model.Document document = new Model.Document();
            JArray jarray;

            try
            {
                document.Id = resource.ResourceId.ToString();

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
                        jarray.Add(new JProperty(resource.UsageRights[i].Entity, (int)resource.UsageRights[i].Permissions));

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
