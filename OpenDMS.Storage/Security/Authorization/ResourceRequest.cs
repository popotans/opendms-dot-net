
namespace OpenDMS.Storage.Security.Authorization
{
    public class ResourceRequest : Request
    {
        public ResourceRequest(string user, Data.ResourceId id, Permissions permissions)
            : base(user, id.ToString(), permissions)
        {
        }

        public ResourceRequest(string user, Data.ResourceId id, GlobalPermission globalPermissions, ResourcePermission resourcePermissions)
            : base(user, id.ToString(), globalPermissions, resourcePermissions)
        {
        }

        public ResourceRequest(string user, Data.ResourceId id, GlobalPermissionType global, ResourcePermissionType resource)
            : base(user, id.ToString(), global, resource)
        {
        }
    }
}
