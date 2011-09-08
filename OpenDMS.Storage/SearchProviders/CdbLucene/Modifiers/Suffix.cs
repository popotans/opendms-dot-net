using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public abstract class Suffix : Base
    {
        public Suffix(string key)
            : base(key)
        {
        }

        public Suffix(string key, string argument)
            : base(key, argument)
        {
        }

        public override string Stringify()
        {
            return Key + Argument;
        }
    }
}
