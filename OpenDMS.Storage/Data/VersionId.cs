using System;

namespace OpenDMS.Storage.Data
{
    public class VersionId
    {
        public ResourceId ResourceId { get; private set; }
        public long VersionNumber { get; private set; }

        public VersionId(ResourceId resourceId)
            : this(resourceId, 0)
        {
        }

        public VersionId(ResourceId resourceId, long versionNumber)
        {
            ResourceId = resourceId;
            VersionNumber = versionNumber;
        }

        public VersionId(Guid resourceId, long versionNumber)
        {
            ResourceId = new ResourceId(resourceId);
            VersionNumber = versionNumber;
        }

        public VersionId(string resourceId, long versionNumber)
        {
            ResourceId = new ResourceId(resourceId);
            VersionNumber = versionNumber;
        }

        public void IncrementVersion()
        {
            VersionNumber++;
        }

        public override string ToString()
        {
            return ResourceId.ToString() + "-" + VersionNumber.ToString();
        }
    }
}
