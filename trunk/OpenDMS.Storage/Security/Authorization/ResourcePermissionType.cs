
namespace OpenDMS.Storage.Security.Authorization
{
    [System.Flags]
    public enum ResourcePermissionType : int
    {
        None = 0x0,

        ReadOnly = 0x1,
        Checkout = 0x2,
        Modify = 0x4,
        VersionControl = 0x8,
        Delete = 0x16,
        All = ReadOnly | Checkout | Modify | VersionControl | Delete
    }
}
