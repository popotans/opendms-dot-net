using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public class Proximity : Suffix
    {
        public Proximity(int distance)
            : base("~", distance.ToString())
        {
        }
    }
}
