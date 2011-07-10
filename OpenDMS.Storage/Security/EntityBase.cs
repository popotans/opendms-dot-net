using System.Collections.Generic;

namespace OpenDMS.Storage.Security
{
    public abstract class EntityBase
    {
        public string Id { get; protected set; }
        public string Rev { get; protected set; }
        public List<string> Groups { get; private set; }

        public EntityBase(string rev, List<string> groups)
        {
            Rev = rev;
            Groups = groups;

            if (groups == null)
                return;

            for (int i = 0; i < Groups.Count; i++)
            {
                if (!Groups[i].StartsWith("group-"))
                    Groups[i] = "group-" + Groups[i];
            }
        }

        public abstract override string ToString();
    }
}
