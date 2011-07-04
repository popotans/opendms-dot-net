using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class GlobalUsageRights
    {
        public GlobalUsageRights()
        {
        }

        public IGlobalUsageRights Transition(Model.Document document)
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
                        prop = (JProperty)jarray[i];
                        usageRight = new Security.UsageRight(prop.Name, (Security.Authorization.ResourcePermissionType)prop.Value<int>());
                        usageRights.Add(usageRight);
                    }

                    document.Remove("UsageRights");
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }

            return new CouchDB.GlobalUsageRights(document.Rev, usageRights);
        }

        public Model.Document Transition(IGlobalUsageRights globals)
        {
            Model.Document document = new Model.Document();
            JArray jarray;

            try
            {
                document.Id = globals.Id;

                if (!string.IsNullOrEmpty(globals.Revision))
                    document.Rev = globals.Revision;

                document["Type"] = "globalusagerights";

                if (globals.UsageRights != null && globals.UsageRights.Count > 0)
                {
                    jarray = new JArray();

                    for (int i = 0; i < globals.UsageRights.Count; i++)
                    {
                        JObject jobj = new JObject();
                        jobj.Add(new JProperty(globals.UsageRights[i].Entity, (int)globals.UsageRights[i].Permissions.Global.Permissions));
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
