
namespace OpenDMS.Storage.Security.Authorization
{
    [System.Flags]
    public enum ResourcePermissionType : int
    {
        None = 0x0,

        // Flags are given slack between bitflags to allow for changes.

        ReadOnly = 0x10,
        Checkout = 0x100,
        Modify = 0x1000,
        VersionControl = 0x10000,
        Delete = 0x100000,
        All = ReadOnly | Checkout | Modify | VersionControl | Delete
    }
}
