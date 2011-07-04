

namespace OpenDMS.Storage.Security
{
    public class UsageRight
    {
        public string Entity { get; private set; }
        public Authorization.Permissions Permissions { get; private set; }

        public bool IsUser { get { return Entity.StartsWith("user-"); } }
        public bool IsGroup { get { return Entity.StartsWith("group-"); } }

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

        public UsageRight(User entity, Authorization.ResourcePermissionType permissions)
            : this(entity.Id, new Authorization.Permissions(null, new Authorization.ResourcePermission(permissions)))
        {
        }

        public UsageRight(Group entity, Authorization.ResourcePermissionType permissions)
            : this(entity.Id, new Authorization.Permissions(null, new Authorization.ResourcePermission(permissions)))
        {
        }

        public UsageRight(User entity, Authorization.GlobalPermissionType permissions)
            : this(entity.Id, new Authorization.Permissions(new Authorization.GlobalPermission(permissions), null))
        {
        }

        public UsageRight(Group entity, Authorization.GlobalPermissionType permissions)
            : this(entity.Id, new Authorization.Permissions(new Authorization.GlobalPermission(permissions), null))
        {
        }

        public bool HasFlags(Authorization.GlobalPermissionType flags)
        {
            return Permissions.HasFlag(flags);
        }

        public bool HasFlags(Authorization.ResourcePermissionType flags)
        {
            return Permissions.HasFlag(flags);
        }
    }
}
