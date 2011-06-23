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

            totalRows = view["total_rows"].Value<int>();

            if (totalRows <= 0)
                return groups;

            rows = (JArray)view["rows"];

            for (int i = 0; i < rows.Count; i++)
            {
                transitionGroup = new Group();
                doc = (Model.Document)rows[i]["key"];
                groups.Add(transitionGroup.Transition(doc));
            }

            return groups;
        }

        public List<Model.Document> Transition(List<Security.Group> groups)
        {
            List<Model.Document> docs = new List<Model.Document>();
            Group transitionGroup;

            if (groups.Count <= 0)
                return docs;

            for (int i = 0; i < groups.Count; i++)
            {
                transitionGroup = new Group();
                docs.Add(transitionGroup.Transition(groups[i]));
            }

            return docs;
        }
    }
}
