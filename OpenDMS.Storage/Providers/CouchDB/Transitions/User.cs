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
                groups);
        }

        public Model.Document Transition(Security.User user)
        {
            Model.Document doc = new Model.Document();
            JArray jarray = new JArray();

            doc.Id = user.Id;
            doc.Rev = user.Rev;
            doc["Password"] = user.Password;
            doc["FirstName"] = user.FirstName;
            doc["MiddleName"] = user.MiddleName;
            doc["LastName"] = user.LastName;

            for (int i=0; i<user.Groups.Count; i++)
                jarray.Add(user.Groups[i]);

            doc["Groups"] = jarray;

            return doc;
        }
    }
}
