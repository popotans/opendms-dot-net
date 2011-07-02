
namespace OpenDMS.Storage.Providers
{
    public interface IGlobalUsageRights
    {
        string Id { get; }
        string Revision { get; }
        System.Collections.Generic.List<OpenDMS.Storage.Security.UsageRight> UsageRights { get; }
    }
}
