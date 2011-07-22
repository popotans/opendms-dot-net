using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetResource : Base
    {
        private Data.ResourceId _resourceId;

        public GetResource(EngineRequest request, Data.ResourceId resourceId)
            : base(request)
        {
            _resourceId = resourceId;
        }

        public override void Execute()
        {
            Commands.GetDocument cmd;

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.GettingResource, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

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
            //AttachSubscriberEvent(cmd, _onProgress);
            //AttachSubscriberEvent(cmd, _onComplete);
            //AttachSubscriberEvent(cmd, _onError);
            //AttachSubscriberEvent(cmd, _onTimeout);
            cmd.OnComplete += new Commands.Base.CompletionDelegate(cmd_OnComplete);
            cmd.OnError += new Commands.Base.ErrorDelegate(cmd_OnError);
            cmd.OnProgress += new Commands.Base.ProgressDelegate(cmd_OnProgress);
            cmd.OnTimeout += new Commands.Base.TimeoutDelegate(cmd_OnTimeout);
            
            try
            {
                cmd.Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }

        void cmd_OnTimeout(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection)
        {
            _onTimeout(_request);
        }

        void cmd_OnProgress(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            _onProgress(_request, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

        void cmd_OnError(Commands.Base sender, Networking.Http.Client client, string message, Exception exception)
        {
            _onError(_request, message, exception);
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
            if (!GetResourcePermissions_OnComplete_IsAuthorized(_request, reply, Security.Authorization.ResourcePermissionType.Checkout))
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
