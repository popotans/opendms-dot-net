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

        public VersionId(string resourceAndVersion)
        {
            long vnum;
            string[] str = resourceAndVersion.Split('-');

            if (str.Length != 2)
                throw new ArgumentException("Invalid argument.");

            ResourceId = new ResourceId(str[0]);

            if(!long.TryParse(str[1], out vnum))
                throw new ArgumentException("Version number could not be parsed.");

            VersionNumber = vnum;
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
