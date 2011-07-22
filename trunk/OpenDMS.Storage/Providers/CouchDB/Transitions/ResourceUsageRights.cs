using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class ResourceUsageRights
    {
        public ResourceUsageRights()
        {
        }

        public IResourceUsageRightsTemplate Transition(Model.Document document)
        {
            List<Security.UsageRight> usageRights = null;
            Security.UsageRight usageRight = null;
            JProperty prop = null;

            try
            {

                if (document["UsageRights"] != null)
                {
                    usageRights = new List<Security.UsageRight>();
                    JArray jarray = (JArray)document["UsageRights"];

                    for (int i = 0; i < jarray.Count; i++)
                    {
                        JObject jobj = (JObject)jarray[i];
                        IEnumerator<JToken> en = jobj.Children().GetEnumerator();
                        while (en.MoveNext())
                        {
                            prop = (JProperty)en.Current;

                            // Json.Net is giving me some weird errors here when I try to call prop.value<int>();
                            // I cannot figure out why so this code is a temporary work-around, it needs figured out.
                            string a = prop.ToString();
                            a = a.Substring(a.LastIndexOf("\"") + 1); // we know the value is an int, so we can look for the last "
                            a = a.Replace(":", "").Trim();

                            usageRight = new Security.UsageRight(prop.Name, (Security.Authorization.ResourcePermissionType)int.Parse(a));
                        }
                        usageRights.Add(usageRight);
                    }

                    //document.Remove("UsageRights");
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            return new CouchDB.ResourceUsageRightsTemplate(document.Rev, usageRights);
        }

        public Model.Document Transition(IResourceUsageRightsTemplate rights)
        {
            Model.Document document = new Model.Document();
            JArray jarray;

            try
            {
                document.Id = rights.Id;

                if (!string.IsNullOrEmpty(rights.Revision))
                    document.Rev = rights.Revision;

                document["Type"] = "resourceusagerightstemplate";

                if (rights.UsageRights != null && rights.UsageRights.Count > 0)
                {
                    jarray = new JArray();

                    for (int i = 0; i < rights.UsageRights.Count; i++)
                    {
                        JObject jobj = new JObject();
                        jobj.Add(new JProperty(rights.UsageRights[i].Entity, (int)rights.UsageRights[i].Permissions.Resource.Permissions));
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
    }
}
