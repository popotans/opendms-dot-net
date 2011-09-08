using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public class Boost : Suffix
    {
        public Boost(int factor)
            : base("^", factor.ToString())
        {
        }
    }
}
