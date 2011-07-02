using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class GlobalUsageRights : OpenDMS.Storage.Providers.IGlobalUsageRights
    {
        public string Id { get { return "globalusagerights"; } }
        public string Revision { get; private set; }
        public List<Security.UsageRight> UsageRights { get; private set; }

        public GlobalUsageRights(string rev, List<Security.UsageRight> usageRights)
        {
            Revision = rev;
            UsageRights = usageRights;
        }
    }
}
