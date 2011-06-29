
namespace OpenDMS.Storage.Security.Authorization
{
    public class GlobalPermission
    {
        public GlobalPermissionType Permissions { get; private set; }

        public bool CanCreate { get { return Permissions.HasFlag(GlobalPermissionType.Create); } }
        public bool CanStatistics { get { return Permissions.HasFlag(GlobalPermissionType.Statistics); } }

        public GlobalPermission(GlobalPermissionType permissions)
        {
            Permissions = permissions;
        }
    }
}
