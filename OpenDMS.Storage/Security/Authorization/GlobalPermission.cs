
namespace OpenDMS.Storage.Security.Authorization
{
    public class GlobalPermission
    {
		#region Constructors (1) 

        public GlobalPermission(GlobalPermissionType permissions)
        {
            Permissions = permissions;
        }

		#endregion Constructors 

		#region Properties (14) 

        public bool CanAll { get { return Permissions.HasFlag(GlobalPermissionType.All); } }

        public bool CanAllGroup { get { return Permissions.HasFlag(GlobalPermissionType.AllGroups); } }

        public bool CanAllUsers { get { return Permissions.HasFlag(GlobalPermissionType.AllUsers); } }

        public bool CanCreate { get { return Permissions.HasFlag(GlobalPermissionType.CreateResource); } }

        public bool CanCreateGroup { get { return Permissions.HasFlag(GlobalPermissionType.CreateGroup); } }

        public bool CanCreateUser { get { return Permissions.HasFlag(GlobalPermissionType.CreateUser); } }

        public bool CanDeleteGroup { get { return Permissions.HasFlag(GlobalPermissionType.DeleteGroup); } }

        public bool CanDeleteUser { get { return Permissions.HasFlag(GlobalPermissionType.DeleteUser); } }

        public bool CanListGroup { get { return Permissions.HasFlag(GlobalPermissionType.ListGroups); } }

        public bool CanListUsers { get { return Permissions.HasFlag(GlobalPermissionType.ListUsers); } }

        public bool CanModifyGroup { get { return Permissions.HasFlag(GlobalPermissionType.ModifyGroup); } }

        public bool CanModifyUser { get { return Permissions.HasFlag(GlobalPermissionType.ModifyUser); } }

        public bool CanStatistics { get { return Permissions.HasFlag(GlobalPermissionType.Statistics); } }

        public GlobalPermissionType Permissions { get; private set; }

		#endregion Properties 
    }
}
