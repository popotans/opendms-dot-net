using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public abstract class Interm : Base
    {
        public Interm(string key)
            : base(key)
        {
        }

        public Interm(string key, string argument)
            : base(key, argument)
        {
        }

        public override string Stringify()
        {
            return "";
        }
    }
}
