using System;
using System.Collections.Generic;

namespace OpenDMS.Security
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }

        public User(string username, string password, string displayname)
        {
            Username = username;
            Password = password;
            DisplayName = displayname;
        }
    }
}
