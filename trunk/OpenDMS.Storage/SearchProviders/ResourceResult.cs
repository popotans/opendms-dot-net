using System;
using System.Collections.Generic;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.SearchProviders
{
    public class ResourceResult : Data.Resource
    {
        public ResourceResult(ResourceId resourceId, string revision, List<VersionId> versionIds,
            VersionId currentVersionId, Metadata metadata, List<Security.UsageRight> usageRights)
            : base(resourceId, revision, versionIds, currentVersionId, metadata, usageRights)
        {
        }
    }
}
