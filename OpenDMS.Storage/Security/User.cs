using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Security
{
    public class User : EntityBase
    {
        private string _password;


        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string LastName { get; private set; }
        public bool IsSuperuser { get; private set; }
        public string Username 
        {
            get
            {
                return Id.Substring(5);
            }
            protected set
            {
                if (value.StartsWith("user-"))
                    Id = value;
                else
                    Id = "user-" + value;
            }
        }
        public string Password
        {
            get { return _password; }
            private set
            {
                System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                _password = System.Convert.ToBase64String(sha512.ComputeHash(bytes));
            }
        }

        public User(string id)
            : base(null, null)
        {
            Username = id;
        }

        public User(string id, string rev, string encryptedPassword, string firstname, string middlename, string lastname, List<string> groups, bool isSuperuser)
            : base(rev, groups)
        {
            Username = id;
            Password = encryptedPassword;
            FirstName = firstname;
            MiddleName = middlename;
            LastName = lastname;
            IsSuperuser = isSuperuser;
        }
    }
}
