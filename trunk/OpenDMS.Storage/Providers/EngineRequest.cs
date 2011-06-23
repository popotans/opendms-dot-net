

namespace OpenDMS.Storage.Providers
{
    public class EngineRequest
    {
        // Events

        public EngineBase.ActionDelegate OnActionChanged { get; set; }
        public EngineBase.ProgressDelegate OnProgress { get; set; }
        public EngineBase.CompletionDelegate OnComplete { get; set; }
        public EngineBase.TimeoutDelegate OnTimeout { get; set; }
        public EngineBase.ErrorDelegate OnError { get; set; }

        public Security.RequestingPartyType RequestingPartyType { get; set; }
        public Security.Session Session { get; set; }
    }
}
