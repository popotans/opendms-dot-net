using System.Collections.Generic;

namespace OpenDMS.Storage.Security
{
    public class Group : EntityBase
    {
        private string _id;
        public override string Id
        {
            get { return _id.Substring(6); }
            protected set
            {
                if (value.StartsWith("group-"))
                    _id = value;
                else
                    _id = "group-" + value;
            }
        }
        public List<string> Users { get; private set; }

        public Group(string id, string rev, List<string> users, List<string> groups)
            : base(rev, groups)
        {
            Id = id;
            Users = users;
        }
    }
}
