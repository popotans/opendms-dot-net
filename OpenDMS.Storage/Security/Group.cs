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

            if (users == null)
                return;

            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].StartsWith("user-"))
                    Users[i] = Users[i].Substring(5);
            }
        }

        public bool UserIsMember(string username)
        {
            if (username.StartsWith("user-"))
                username = username.Substring(5);

            for (int i = 0; i < Users.Count; i++)
                if (Users[i] == username)
                    return true;

            return false;
        }

        public override string ToString()
        {
            return GroupName;
        }
    }
}
