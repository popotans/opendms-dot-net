
namespace OpenDMS.Storage.Providers
{
    public enum EngineActionType
    {
        Preparing,
        Getting,
        GettingGroups,
        GettingGroup,
        GettingUser,
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
        // No need to send a complete action as completion is signaled by Error, Timeout or Completion
        // Complete
    }
}
