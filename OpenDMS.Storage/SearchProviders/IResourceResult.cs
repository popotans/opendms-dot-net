using System;
using System.Collections.Generic;
using OpenDMS.Storage.Security;

namespace OpenDMS.Storage.SearchProviders
{
    public interface IResourceResult
    {
        DateTime? CheckedOutAt { get; set; }
        string CheckedOutTo { get; set; }
        DateTime LastCommit { get; set; }
        string LastCommitter { get; set; }
        System.Collections.Generic.List<string> Tags { get; set; }
        string Title { get; set; }
        List<UsageRight> UsageRights { get; set; }

        SearchProviders.ResourceResult MakeResource();
    }
}
