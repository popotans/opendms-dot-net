using System;

namespace Common.NetworkPackage
{
    public class ResourceLock : DictionaryBase<string, object>
    {
        public ResourceLock()
            : base("ResourceLock")
        {
        }

        public bool Validate()
        {
            if (!ContainsKey("Guid")) return false;
            if (!ContainsKey("Timestamp")) return false;
            if (!ContainsKey("Username")) return false;

            if (this["Guid"].GetType() != typeof(Guid)) return false;
            if (this["Timestamp"].GetType() != typeof(DateTime)) return false;
            if (this["Username"].GetType() != typeof(string)) return false;

            return true;
        }
    }
}
