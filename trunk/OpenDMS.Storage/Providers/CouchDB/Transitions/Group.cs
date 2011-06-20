using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class Group
    {
        public Group()
        {
        }

        public Security.Group Transition(Model.Document document)
        {
            List<string> users = null;
            List<string> groups = null;
            JArray usersJray, groupsJray;

            if (document["Groups"] != null)
            {
                groups = new List<string>();
                groupsJray = (JArray)document["Groups"];

                for (int i = 0; i < groupsJray.Count; i++)
                    groups.Add(groupsJray[i].Value<string>());
            }

            if (document["Users"] != null)
            {
                users = new List<string>();
                usersJray = (JArray)document["Users"];

                for (int i = 0; i < usersJray.Count; i++)
                    groups.Add(usersJray[i].Value<string>());
            }
            
            return new Security.Group(document.Id,
                document.Rev,
                users,
                groups);
        }

        public Model.Document Transition(Security.Group group)
        {
            Model.Document doc = new Model.Document();
            JArray groupsJray = new JArray();
            JArray usersJray = new JArray();

            doc.Id = group.Id;
            doc.Rev = group.Rev;

            for (int i = 0; i < group.Groups.Count; i++)
                groupsJray.Add(group.Groups[i]);

            for (int i = 0; i < group.Users.Count; i++)
                usersJray.Add(group.Users[i]);

            doc["Groups"] = groupsJray;
            doc["Users"] = usersJray;

            return doc;
        }
    }
}
