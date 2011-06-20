
namespace OpenDMS.Storage.Providers
{
    public enum EngineActionType
    {
        Preparing,
        CreatingNewResource,
        CreatingNewVersion,
        UpdatingResource,
        RecoveringFromError,
        RecoveringFromTimeout,
        Reverting,
        // No need to send a complete action as completion is signaled by Error, Timeout or Completion
        // Complete
    }
}
