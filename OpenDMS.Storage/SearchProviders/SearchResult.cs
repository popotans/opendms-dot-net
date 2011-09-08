using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders
{
    public class SearchResult
    {
        public List<ResourceResult> Resources { get; set; }
        public List<VersionResult> Versions { get; set; }

        public SearchResult(List<ResourceResult> resources, List<VersionResult> versions)
        {
            Resources = resources;
            Versions = versions;
        }
    }
}
