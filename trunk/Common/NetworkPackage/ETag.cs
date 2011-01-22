using System;

namespace Common.NetworkPackage
{
    public class ETag 
        : DictionaryBase<string, object>
    {
        public ETag()
            : base("ETag")
        {
        }

        public bool Validate()
        {
            if (!ContainsKey("Value")) return false;

            if (this["Value"].GetType() != typeof(string)) return false;

            return true;
        }
    }
}
