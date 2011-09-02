using System;
using System.Collections.Generic;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers
{
    public class CreateResourceArgs
    {
        public CreateVersionArgs VersionArgs { get; set; }

        public List<VersionId> VersionIds { get; set; }
        public VersionId CurrentVersionId { get; set; }
        public Metadata Metadata { get; set; }
        public List<Security.UsageRight> UsageRights { get; set; }

        public List<string> Tags { get; set; }
        public string Title { get; set; }

        public CreateResourceArgs()
        {
        }
    }
}
