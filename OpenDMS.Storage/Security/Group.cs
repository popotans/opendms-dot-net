using System.Collections.Generic;

namespace OpenDMS.Storage.Security
{
    public class Group : EntityBase
    {
        public override string Id { get; protected set; }
        public List<string> Users { get; private set; }

        public Group(string id, string rev, List<string> users, List<string> groups)
            : base(rev, groups)
        {
            Id = id;
            Users = users;
        }
    }
}
