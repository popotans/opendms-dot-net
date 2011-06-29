

namespace OpenDMS.Storage.Security
{
    public class UsageRight
    {
        public string Entity { get; private set; }
        public Authorization.Permissions Permissions { get; private set; }

        public UsageRight(string entity, Authorization.Permissions permissions)
        {
            Entity = entity;
            Permissions = permissions;
        }

        public UsageRight(string entity, Authorization.ResourcePermissionType permissions)
            : this(entity, new Authorization.Permissions(null, new Authorization.ResourcePermission(permissions)))
        {
        }

        public UsageRight(string entity, Authorization.GlobalPermissionType permissions)
            : this(entity, new Authorization.Permissions(new Authorization.GlobalPermission(permissions), null))
        {
        }
    }
}
