using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public class Fuzzy : Suffix
    {
        public Fuzzy(float similarityDistance)
            : base("~", similarityDistance.ToString())
        {
        }
    }
}
