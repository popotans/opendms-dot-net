using System;

namespace Common.NetworkPackage
{
    public class User : DictionaryBase<string, string>
    {
        public User()
            : base("User")
        {
        }
    }
}
