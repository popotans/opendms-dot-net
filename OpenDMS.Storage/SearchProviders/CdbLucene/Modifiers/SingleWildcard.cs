using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public class SingleWildcard : Interm
    {
        public SingleWildcard()
            : base("?")
        {
        }
    }
}
