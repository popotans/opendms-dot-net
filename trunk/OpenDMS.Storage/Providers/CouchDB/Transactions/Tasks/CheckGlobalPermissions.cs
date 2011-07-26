using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class CheckGlobalPermissions : Base
    {
        private IDatabase _db;
        private GlobalUsageRights _gur;
        private Security.RequestingPartyType _rpt;
        private Security.Session _session;
        private Security.Authorization.GlobalPermissionType _requiredPermissions;

        public bool IsAuthorized { get; private set; }

        public CheckGlobalPermissions(IDatabase db, GlobalUsageRights gur, Security.RequestingPartyType requestingPartyType,
            Security.Session session, Security.Authorization.GlobalPermissionType requiredPermissions)
        {
            _db = db;
            _gur = gur;
            _rpt = requestingPartyType;
            _session = session;
            _requiredPermissions = requiredPermissions;
        }

        public override void Process()
        {
            TriggerOnActionChanged(EngineActionType.CheckingPermissions, false);

            if (_rpt == Security.RequestingPartyType.System)
            {
                Logger.Security.Debug("Request from System, granting access without checking global permissions.");
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
            IsAuthorized = Security.Authorization.Manager.IsAuthorized(_gur.UsageRights, _requiredPermissions, groupMembership,
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