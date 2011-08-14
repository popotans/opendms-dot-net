
namespace OpenDMS.Storage.Security.Authorization
{
    [System.Flags]
    public enum GlobalPermissionType : int
    {
        None = 0x0,

        Statistics              = 1,
        CreateResource          = 2,

        GetGlobalPermissions    = 4,
        ModifyGlobalPermissions = 8,

        CreateGroup             = 16,
        ModifyGroup             = 32,
        DeleteGroup             = 64,
        ListGroups              = 128,
        GetGroup                = 256,
        AllGroups               = CreateGroup | ModifyGroup | DeleteGroup | ListGroups | GetGroup,

        CreateUser              = 512,
        ModifyUser              = 1024,
        DeleteUser              = 2048,
        ListUsers               = 4096,
        GetUser                 = 8192,
        AllUsers                = CreateUser | ModifyUser | DeleteUser | ListUsers | GetUser,

        GetResourceUsageRightsTemplate = 16384,
        ModifyResourceUsageRightsTemplate = 32768,

        All = CreateResource | Statistics | AllUsers | AllGroups | GetGlobalPermissions |
            ModifyGlobalPermissions | GetResourceUsageRightsTemplate | ModifyResourceUsageRightsTemplate
    }
}
