

namespace OpenDMS.Storage.Security
{
    public class UsageRight
    {
        public string Entity { get; private set; }
        public PermissionType Permissions { get; private set; }

        public UsageRight(string entity, PermissionType permissions)
        {
            Entity = entity;
            Permissions = permissions;
        }

        public bool IsAllowed(PermissionType permissions)
        {
            return Permissions.HasFlag(permissions);
        }
    }
}
