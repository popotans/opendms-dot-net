using System.Collections.Generic;

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
        public EngineBase.AuthorizationDelegate OnAuthorizationDenied { get; set; }

        public IEngine Engine { get; set; }
        public IDatabase Database { get; set; }
        public Security.RequestingPartyType RequestingPartyType { get; set; }
        public System.Guid AuthToken { get; set; }

        public object UserToken { get; set; }

        public Security.Session Session
        {
            get { return Database.SessionManager.LookupSession(AuthToken); }
        }

        public List<Security.Group> GetGroupMembership()
        {
            return Database.SessionManager.GroupMembershipOfUser(Session.User.Username);
        }
    }
}
