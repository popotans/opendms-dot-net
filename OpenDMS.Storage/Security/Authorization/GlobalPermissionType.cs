
namespace OpenDMS.Storage.Security.Authorization
{
    [System.Flags]
    public enum GlobalPermissionType : int
    {
        None = 0x0,

        // Slack between bitflags to allow for changes.

        Statistics              = 0x000010,
        CreateResource          = 0x000100,

        GetGlobalPermissions    = 0x001000,
        ModifyGlobalPermissions = 0x001001,

        CreateGroup             = 0x010000,
        ModifyGroup             = 0x010001,
        DeleteGroup             = 0x010010,
        ListGroups              = 0x010100,
        GetGroup                = 0x011000,
        AllGroups               = CreateGroup | ModifyGroup | DeleteGroup | ListGroups | GetGroup,

        CreateUser              = 0x100000,
        ModifyUser              = 0x100001,
        DeleteUser              = 0x100010,
        ListUsers               = 0x100100,
        GetUser                 = 0x101000,
        AllUsers                = CreateUser | ModifyUser | DeleteUser | ListUsers | GetUser,

        All = CreateResource | Statistics | AllUsers | AllGroups | GetGlobalPermissions | ModifyGlobalPermissions
    }
}
