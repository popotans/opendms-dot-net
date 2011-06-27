using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class User
    {
        public User()
        {
        }

        public Security.User Transition(Model.Document document)
        {
            List<string> groups = null;

            try
            {
                if (document["Groups"] != null)
                {
                    groups = new List<string>();
                    JArray jarray = (JArray)document["Groups"];

                    for (int i = 0; i < jarray.Count; i++)
                        groups.Add(jarray[i].Value<string>());
                }

                return new Security.User(document.Id,
                    document.Rev,
                    document["Password"].Value<string>(),
                    document["FirstName"].Value<string>(),
                    document["MiddleName"].Value<string>(),
                    document["LastName"].Value<string>(),
                    groups,
                    document["Superuser"].Value<bool>());
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the document.", e);
                throw;
            }
        }

        public Model.Document Transition(Security.User user)
        {
            Model.Document doc = new Model.Document();
            JArray jarray = new JArray();

            try
            {
                doc.Id = user.Id;
                doc.Rev = user.Rev;
                doc["Type"] = "user";
                doc["Password"] = user.Password;
                doc["FirstName"] = user.FirstName;
                doc["MiddleName"] = user.MiddleName;
                doc["LastName"] = user.LastName;
                doc["Superuser"] = user.IsSuperuser;

                for (int i = 0; i < user.Groups.Count; i++)
                    jarray.Add(user.Groups[i]);

                doc["Groups"] = jarray;
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the user.", e);
                throw;
            }

            return doc;
        }
    }
}
