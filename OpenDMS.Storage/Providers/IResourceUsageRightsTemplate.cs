
namespace OpenDMS.Storage.Providers
{
    public interface IResourceUsageRightsTemplate
    {
        string Id { get; }
        string Revision { get; }
        System.Collections.Generic.List<OpenDMS.Storage.Security.UsageRight> UsageRights { get; }
    }
}
