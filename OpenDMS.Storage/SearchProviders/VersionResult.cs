using System;
using System.Collections.Generic;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.SearchProviders
{
    public class VersionResult : Data.Version
    {
        public VersionResult(VersionId versionId, string revision, Metadata metadata, Content content)
            : base(versionId, revision, metadata, content)
        {
        }
    }
}
