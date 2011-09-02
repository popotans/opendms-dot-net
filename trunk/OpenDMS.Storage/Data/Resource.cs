using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Data
{
    public class Resource
    {
        public ResourceId ResourceId { get; private set; }
        public string Revision { get; private set; }
        public List<VersionId> VersionIds { get; private set; }
        public VersionId CurrentVersionId { get; private set; }
        public Metadata Metadata { get; protected set; }
        public List<Security.UsageRight> UsageRights { get; protected set; }

        public List<string> Tags { get; set; }
        public DateTime Created { get; set; }
        public string Creator { get; set; }
        public DateTime Modified { get; set; }
        public string Modifier { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public string CheckedOutTo { get; set; }
        public DateTime LastCommit { get; set; }
        public string LastCommitter { get; set; }
        public string Title { get; set; }

        public Resource()
        {
        }

        public Resource(ResourceId resourceId, string revision, List<VersionId> versionIds, 
            VersionId currentVersionId, Metadata metadata, List<Security.UsageRight> usageRights)
        {
            ResourceId = resourceId;
            Revision = revision;
            VersionIds = versionIds;
            CurrentVersionId = currentVersionId;
            Metadata = metadata;
            UsageRights = usageRights;
        }

        public Resource(ResourceId resourceId, string revision, string checkedOutTo, DateTime? checkedOutAt, 
            List<VersionId> versionIds, VersionId currentVersionId, Metadata metadata, 
            List<Security.UsageRight> usageRights)
            : this(resourceId, revision, versionIds, currentVersionId, metadata, usageRights)
        {
            CheckedOutTo = checkedOutTo;
            CheckedOutAt = checkedOutAt;
        }

        public void UpdateRevision(string revision)
        {
            Revision = revision;
        }

        public void UpdateCheckout(DateTime at, string to)
        {
            CheckedOutAt = at;
            CheckedOutTo = to;
        }

        public void UpdateCurrentVersionBasedOnVersionsList()
        {
            CurrentVersionId = VersionIds[VersionIds.Count - 1];
        }
    }
}
