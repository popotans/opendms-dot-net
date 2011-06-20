using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Security
{
    public class User : EntityBase
    {
        private string _id;
        private string _password;


        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string LastName { get; private set; }
        public override string Id
        {
            get { return _id.Substring(5); }
            protected set
            {
                if (value.StartsWith("user-"))
                    _id = value;
                else
                    _id = "user-" + value;
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

        public User(string id, string rev, string encryptedPassword, string firstname, string middlename, string lastname, List<string> groups)
            : base(rev, groups)
        {
            Id = id;
            Password = encryptedPassword;
            FirstName = firstname;
            MiddleName = middlename;
            LastName = lastname;
        }
    }
}
