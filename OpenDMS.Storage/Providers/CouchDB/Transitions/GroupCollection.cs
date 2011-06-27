using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transitions
{
    public class GroupCollection
    {
        public GroupCollection()
        {
        }

        public List<Security.Group> Transition(Model.View view)
        {
            List<Security.Group> groups = new List<Security.Group>();
            Group transitionGroup;
            Model.Document doc;
            JArray rows;
            int totalRows;

            try
            {
                totalRows = view["total_rows"].Value<int>();

                if (totalRows <= 0)
                    return groups;

                rows = (JArray)view["rows"];

                for (int i = 0; i < rows.Count; i++)
                {
                    transitionGroup = new Group();
                    JObject obj = (JObject)rows[i];
                    JObject a = (JObject)obj["key"];
                    doc = new Model.Document(rows[i]["key"]);
                    groups.Add(transitionGroup.Transition(doc));
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the view.", e);
                throw;
            }

            return groups;
        }

        public List<Model.Document> Transition(List<Security.Group> groups)
        {
            List<Model.Document> docs = new List<Model.Document>();
            Group transitionGroup;

            try
            {
                if (groups.Count <= 0)
                    return docs;

                for (int i = 0; i < groups.Count; i++)
                {
                    transitionGroup = new Group();
                    docs.Add(transitionGroup.Transition(groups[i]));
                }
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while attempting to parse the view.", e);
                throw;
            }

            return docs;
        }
    }
}
