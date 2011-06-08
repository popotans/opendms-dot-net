using System.Collections.Generic;

namespace OpenDMS.Storage.Data
{
    public class Resource
    {
        public ResourceId ResourceId { get; private set; }
        public string Revision { get; private set; }
        public List<VersionId> VersionIds { get; private set; }
        public VersionId CurrentVersionId { get; private set; }

        public Resource()
        {
        }

        public Resource(ResourceId resourceId, string revision, List<VersionId> versionIds, VersionId currentVersionId)
        {
            ResourceId = resourceId;
            Revision = revision;
            VersionIds = versionIds;
            CurrentVersionId = currentVersionId;
        }
    }
}
