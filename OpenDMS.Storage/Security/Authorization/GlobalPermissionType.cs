
namespace OpenDMS.Storage.Security.Authorization
{
    [System.Flags]
    public enum GlobalPermissionType : int
    {
        None = 0x0,

        // Flags are given slack between bitflags to allow for changes.

        Create = 0x10,
        Statistics = 0x100,
        All = Create | Statistics
    }
}
