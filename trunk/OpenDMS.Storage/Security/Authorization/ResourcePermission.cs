
namespace OpenDMS.Storage.Security.Authorization
{
    public class ResourcePermission
    {
        public ResourcePermissionType Permissions { get; private set; }

        public bool CanReadOnly { get { return Permissions.HasFlag(ResourcePermissionType.ReadOnly); } }
        public bool CanCheckout { get { return Permissions.HasFlag(ResourcePermissionType.Checkout); } }
        public bool CanModify { get { return Permissions.HasFlag(ResourcePermissionType.Modify); } }
        public bool CanVersionControl { get { return Permissions.HasFlag(ResourcePermissionType.VersionControl); } }
        public bool CanDelete { get { return Permissions.HasFlag(ResourcePermissionType.Delete); } }

        public ResourcePermission(ResourcePermissionType permissions)
        {
            Permissions = permissions;
        }
    }
}
