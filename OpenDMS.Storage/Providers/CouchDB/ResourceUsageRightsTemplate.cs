using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class ResourceUsageRightsTemplate : OpenDMS.Storage.Providers.IResourceUsageRightsTemplate
    {
        public string Id { get { return "resourceusagerightstemplate"; } }
        public string Revision { get; private set; }
        public List<Security.UsageRight> UsageRights { get; private set; }

        public ResourceUsageRightsTemplate(string rev, List<Security.UsageRight> usageRights)
        {
            Revision = rev;
            UsageRights = usageRights;
        }
    }
}
