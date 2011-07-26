using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class CheckResourcePermissions : Base
    {
        private IDatabase _db;
        private Data.Resource _resource;
        private Security.RequestingPartyType _rpt;
        private Security.Session _session;
        private Security.Authorization.ResourcePermissionType _requiredPermissions;

        public bool IsAuthorized { get; private set; }

        public CheckResourcePermissions(IDatabase db, Data.Resource resource, Security.RequestingPartyType requestingPartyType,
            Security.Session session, Security.Authorization.ResourcePermissionType requiredPermissions)
        {
            _db = db;
            _resource = resource;
            _rpt = requestingPartyType;
            _session = session;
            _requiredPermissions = requiredPermissions;
        }

        public override void Process()
        {
            TriggerOnActionChanged(EngineActionType.CheckingPermissions, false);

            if (_rpt == Security.RequestingPartyType.System)
            {
                Logger.Security.Debug("Request from System, granting access without checking resource permissions.");
                IsAuthorized = true;
                TriggerOnComplete(null);
                return;
            }

            if (_session == null || _session.User == null)
                throw new ArgumentNullException("The session argument and its user property must be non-null.");

            if (_session.User.IsSuperuser)
            {
                Logger.Security.Debug("Request from Superuser '" + _session.User.Username + "', granting access without checking permissions.");
                IsAuthorized = true;
                TriggerOnComplete(null);
                return;
            }

            List<Security.Group> groupMembership = _db.SessionManager.GroupMembershipOfUser(_session.User.Username);
            IsAuthorized = Security.Authorization.Manager.IsAuthorized(_resource.UsageRights, _requiredPermissions, groupMembership,
                _session.User.Username);

            if (!IsAuthorized)
            {
                TriggerOnAuthorizationDenied();
                return;
            }

            TriggerOnComplete(null);
        }
    }
}
