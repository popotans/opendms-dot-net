using System.Collections.Generic;

namespace OpenDMS.Storage.Security
{
    public class Group : EntityBase
    {
        public string GroupName
        {
            get
            {
                return Id.Substring(6);
            }
            protected set
            {
                if (value.StartsWith("group-"))
                    Id = value;
                else
                    Id = "group-" + value;
            }
        }
        public List<string> Users { get; private set; }

        public Group(string id, string rev, List<string> users, List<string> groups)
            : base(rev, groups)
        {
            GroupName = id;
            Users = users;
        }
    }
}
