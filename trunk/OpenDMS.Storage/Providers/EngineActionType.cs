
namespace OpenDMS.Storage.Providers
{
    public enum EngineActionType
    {
        None,
        Preparing,
        Getting,
        GettingGroups,
        GettingGroup,
        GettingUser,
        GettingUsers,
        Aborting,
        SessionLookup,
        CheckingPermissions,
        CreatingUser,
        CreatingGlobalUsageRights,
        GettingGlobalUsageRights,
        CreatingGroup,
        DeterminingInstallation,
        AuthenticatingUser,
        Installing,
        ModifyingGroup,
        ModifyingUser,
        GettingResourceUsageRightsTemplate,
        CreatingNewResource,
        CreatingNewVersion,
        ModifyingResource,
        GettingResource,
        UploadingBulk,
        CheckingExistance
        // No need to send a complete action as completion is signaled by Error, Timeout or Completion
        // Complete
    }
}
