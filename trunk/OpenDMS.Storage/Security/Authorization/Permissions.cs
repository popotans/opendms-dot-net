
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
    }
}
