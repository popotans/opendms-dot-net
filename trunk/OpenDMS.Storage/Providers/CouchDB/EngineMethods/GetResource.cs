using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetResource : Base
    {
        private Data.ResourceId _resourceId;
        private bool _readonly;

        public GetResource(EngineRequest request, Data.ResourceId resourceId, bool readOnly)
            : base(request)
        {
            _resourceId = resourceId;
            _readonly = readOnly;
        }

        public override void Execute()
        {
            // Normal
            //  1) Request a local lock
            //      A) This is going to require a local locking system that will apply a lock until it is applied to the resource on the database
            //          i) Will need to return some sort of authorization event indicating the resource is in use
            //  2) Download resource
            //  3) Check resource permissions (can this user access it?)
            //  4) Check resource lock (is it checked out?)
            //  5) ModifyResource to include a lock
            //      A) if failure, needs to release local lock then fire OnError
            //  6) Release local lock
            //  7) Give implementing software the resource
            // ReadOnly
            //  1) Download resource
            //  2) Check permissions
            //  3) Give implementing software the resource

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.GettingResource, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            if (_readonly)
                DoReadOnly();
            else
                DoCheckout();
        }

        private void DoCheckout()
        {
            Transactions.Actions.GetResource t = new Transactions.Actions.GetResource(_request.Database, 
                _request.Engine, _request.AuthToken, _resourceId, _request.Session.User.Username);
            t.e
        }

        private void DoReadOnly()
        {
            Commands.GetDocument cmd;

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(_request.Database, _resourceId));
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDocument command.", e);
                throw;
            }

            // Run it straight back to the subscriber
            AttachSubscriberEvent(cmd, _onProgress);
            //AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);
            cmd.OnComplete += new Commands.Base.CompletionDelegate(cmd_OnComplete);
            
            try
            {
                cmd.Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }

        private void cmd_OnComplete(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Commands.ReplyBase reply)
        {
            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.SessionLookup, false);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            // If the requesting party is not the system, we need to get a session
            // System is immune to session checking.
            if (_request.RequestingPartyType != Security.RequestingPartyType.System)
            {
                if (_request.Session == null)
                {
                    Logger.Security.Error("Request to create user failed as the specified authentication token could not be paired with a session.");
                    try
                    {
                        _onError(_request, "No session match.", null);
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the OnError event.", e);
                        throw;
                    }
                }
                else if (_request.Session.User.IsSuperuser)
                { // Superuser found - pass without checking permissions
                    Logger.Security.Debug("Request from Superuser '" + _request.Session.User.Username + "', bypassing permissions check.");
                    // Fall-thru
                }
            }

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CheckingPermissions, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }
                        
            // Check permissions
            if (!GetResourcePermissions_OnComplete_IsAuthorized(_request, reply, Security.Authorization.ResourcePermissionType.ReadOnly))
                return;

            try { _onComplete(_request, reply); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onComplete argument.", e);
                throw;
            }
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            throw new NotImplementedException();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            throw new NotImplementedException();
        }
    }
}
