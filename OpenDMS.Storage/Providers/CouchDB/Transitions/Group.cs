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

            try
            {
                if (document["Groups"] != null)
                {
                    groups = new List<string>();
                    groupsJray = (JArray)document["Groups"];

                    for (int i = 0; i < groupsJray.Count; i++)
                        groups.Add(groupsJray[i].Value<string>());
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse groups.", e);
                throw;
            }

            try
            {
                if (document["Users"] != null)
                {
                    users = new List<string>();
                    usersJray = (JArray)document["Users"];

                    for (int i = 0; i < usersJray.Count; i++)
                        groups.Add(usersJray[i].Value<string>());
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse users.", e);
                throw;
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

            try
            {
                doc.Id = group.Id;
                if (group.Rev != null)
                    doc.Rev = group.Rev;
                doc["Type"] = "group";

                if (group.Groups != null)
                {
                    for (int i = 0; i < group.Groups.Count; i++)
                        groupsJray.Add(group.Groups[i]);
                }

                if (group.Users != null)
                {
                    for (int i = 0; i < group.Users.Count; i++)
                        usersJray.Add(group.Users[i]);
                }

                doc["Groups"] = groupsJray;
                doc["Users"] = usersJray;
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the group object.", e);
                throw;
            }

            return doc;
        }
    }
}
