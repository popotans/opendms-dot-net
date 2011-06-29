
namespace OpenDMS.Storage.Security.Authorization
{
    public abstract class Request
    {
        public string User { get; protected set; }
        public string Id { get; protected set; }
        public Permissions Permissions { get; protected set; }

        public Request(string user, string id, Permissions permissions)
        {
            User = user;
            Id = id;
            Permissions = permissions;
        }

        public Request(string user, string id, GlobalPermission globalPermissions, ResourcePermission resourcePermissions)
            : this(user, id, new Permissions(globalPermissions, resourcePermissions))
        {
        }

        public Request(string user, string id, GlobalPermissionType global, ResourcePermissionType resource)
            : this(user, id, new GlobalPermission(global), new ResourcePermission(resource))
        {
        }
    }
}
