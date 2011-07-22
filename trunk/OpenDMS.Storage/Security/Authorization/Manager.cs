using System.Collections.Generic;

namespace OpenDMS.Storage.Security.Authorization
{
    public class Manager
    {
        public static bool IsAuthorized(List<UsageRight> usageRights, ResourcePermissionType requiredPermissions, List<Group> groupMembership, string username)
        {
            List<UsageRight> applicableGroupRights;
            UsageRight userRights;
            ResourcePermissionType cumulativeResourceRights = ResourcePermissionType.None;

            // User rights override group rights when specified, thus we want to check user rights first and bail with the answer if possible
            userRights = GetRightsOfUser(usageRights, username);

            if (userRights != null)
            {
                // If the user has the required rights, we are done.
                if (userRights.HasFlags(requiredPermissions))
                    return true;
                else // Now, if the user has any rights, we know it is denied because rights are either set or not set as a whole
                    return false;
            }

            // So, if we are here, the user does not have specific rights, we need to check group rights
            applicableGroupRights = GetRightsOfGroupsToWhichUserBelongs(usageRights, groupMembership);

            // If nothing is applicable then we know access is denied.
            if (applicableGroupRights == null)
                return false;

            /* Now, group rights are granting cumulative.  For instance, if a user is a member of groups A and B where A has readonly access and B has only checkout access then
             * the user would have both readonly and checkout access.  Only user flags are applied to decline access as the system defaults to no access.  Thus the simple way to 
             * look at it is that by making a user a member of a group you are granting the specified permissions of that group.
             */

            // So, we need to cumulate permissions, this is easily accomplished with a bitwise or operation
            for (int i = 0; i < applicableGroupRights.Count; i++)
                if (applicableGroupRights[i].Permissions.Resource != null)
                    cumulativeResourceRights |= applicableGroupRights[i].Permissions.Resource.Permissions;

            return cumulativeResourceRights.HasFlag(requiredPermissions);
        }

        public static bool IsAuthorized(List<UsageRight> usageRights, GlobalPermissionType requiredPermissions, List<Group> groupMembership, string username)
        {
            List<UsageRight> applicableGroupRights;
            UsageRight userRights;
            GlobalPermissionType cumulativeGroupRights = GlobalPermissionType.None;

            // User rights override group rights when specified, thus we want to check user rights first and bail with the answer if possible
            userRights = GetRightsOfUser(usageRights, username);

            if (userRights != null)
            {
                // If the user has the required rights, we are done.
                if (userRights.HasFlags(requiredPermissions))
                    return true;
                else // Now, if the user has any rights, we know it is denied because rights are either set or not set as a whole
                    return false;
            }
            
            // So, if we are here, the user does not have specific rights, we need to check group rights
            applicableGroupRights = GetRightsOfGroupsToWhichUserBelongs(usageRights, groupMembership);

            // If nothing is applicable then we know access is denied.
            if (applicableGroupRights == null)
                return false;

            /* Now, group rights are granting cumulative.  For instance, if a user is a member of groups A and B where A has readonly access and B has only checkout access then
             * the user would have both readonly and checkout access.  Only user flags are applied to decline access as the system defaults to no access.  Thus the simple way to 
             * look at it is that by making a user a member of a group you are granting the specified permissions of that group.
             */

            // So, we need to cumulate permissions, this is easily accomplished with a bitwise or operation
            for (int i = 0; i < applicableGroupRights.Count; i++)
                cumulativeGroupRights |= applicableGroupRights[i].Permissions.Global.Permissions;

            return cumulativeGroupRights.HasFlag(requiredPermissions);
        }

        private static UsageRight GetRightsOfUser(List<UsageRight> allRights, string username)
        {
            for (int i = 0; i < allRights.Count; i++)
            {
                if (allRights[i].Entity == "user-" + username)
                    return allRights[i];
            }

            return null;
        }

        private static List<UsageRight> GetRightsOfGroupsToWhichUserBelongs(List<UsageRight> allRights, List<Group> groupMembership)
        {
            List<UsageRight> groupRights = GetGroupRights(allRights);
            List<UsageRight> output = new List<UsageRight>();

            for (int i=0; i<groupMembership.Count; i++)
            {
                for (int j=0; j<groupRights.Count; j++)
                {
                    if (groupMembership[i].Id == groupRights[j].Entity)
                    {
                        output.Add(groupRights[j]);
                        break;
                    }
                }
            }

            return output;
        }

        private static List<UsageRight> GetGroupRights(List<UsageRight> allRights)
        {
            List<UsageRight> groupRights = new List<UsageRight>();
            for(int i=0; i<allRights.Count; i++)
            {
                if(allRights[i].IsGroup)
                    groupRights.Add(allRights[i]);
            }
            return groupRights;
        }

        private static List<UsageRight> GetUserRights(List<UsageRight> allRights)
        {
            List<UsageRight> userRights = new List<UsageRight>();
            for(int i=0; i<allRights.Count; i++)
            {
                if(allRights[i].IsUser)
                    userRights.Add(allRights[i]);
            }
            return userRights;
        }
    }
}
