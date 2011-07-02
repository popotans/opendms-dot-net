
namespace OpenDMS.Storage.Security.Authorization
{
    public class Permissions
    {
        public GlobalPermission Global { get; private set; }
        public ResourcePermission Resource { get; private set; }
        
        public Permissions(GlobalPermission global, ResourcePermission resource)
        {
            Global = global;
            Resource = resource;
        }

        public bool HasFlag(GlobalPermissionType flags)
        {
            if (Global != null)
                return Global.Permissions.HasFlag(flags);
            return false;
        }

        public bool HasFlag(ResourcePermissionType flags)
        {
            if (Resource != null)
                return Resource.Permissions.HasFlag(flags);
            return false;
        }
    }
}
