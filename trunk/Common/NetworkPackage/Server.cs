using System;
using System.Collections.Generic;

namespace Common.NetworkPackage
{
    public class Server 
        : DictionaryBase<string, object>
    {
        public Server()
            : base("Server")
        {
        }

        public bool Validate()
        {
            if (!ContainsKey("Name")) return false;
            if (!ContainsKey("Location")) return false;
            if (!ContainsKey("Address")) return false;
            if (!ContainsKey("Port")) return false;

            if (this["Name"].GetType() != typeof(string)) return false;
            if (this["Location"].GetType() != typeof(string)) return false;
            if (this["Address"].GetType() != typeof(string)) return false;
            if (this["Port"].GetType() != typeof(int)) return false;

            return true;
        }
    }
}
