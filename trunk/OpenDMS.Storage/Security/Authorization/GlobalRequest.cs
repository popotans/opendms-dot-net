
namespace OpenDMS.Storage.Security.Authorization
{
    public class GlobalRequest : Request
    {
        private const string GLOBAL_FILE_ID = "globaluserrights";
                
        public GlobalRequest(string user, Permissions permissions)
            : base(user, GLOBAL_FILE_ID, permissions)
        {
        }

        public GlobalRequest(string user, GlobalPermission globalPermissions, ResourcePermission resourcePermissions)
            : base(user, GLOBAL_FILE_ID, globalPermissions, resourcePermissions)
        {
        }

        public GlobalRequest(string user, GlobalPermissionType global, ResourcePermissionType resource)
            : base(user, GLOBAL_FILE_ID, global, resource)
        {
        }
    }
}
