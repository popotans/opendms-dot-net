using System;

namespace OpenDMS.Storage.SearchProviders
{
    public interface IVersionResult
    {
        string Extension { get; set; }
        string Md5 { get; set; }

        SearchProviders.VersionResult MakeVersion();
    }
}
