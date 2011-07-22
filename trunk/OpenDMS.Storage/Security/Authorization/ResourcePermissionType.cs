
namespace OpenDMS.Storage.Security.Authorization
{
    [System.Flags]
    public enum ResourcePermissionType : int
    {
        None = 0x0,

        ReadOnly = 1,
        Checkout = 2,
        Modify = 4,
        VersionControl = 8,
        Delete = 16,
        All = ReadOnly | Checkout | Modify | VersionControl | Delete
    }
}
