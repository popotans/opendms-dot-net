
namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetGlobalPermissions : Base
    {
        public GetGlobalPermissions(EngineRequest request)
            : base(request)
        {
        }

        public override void Execute()
        {
            Commands.GetDocument cmd;

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

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(_request.Database, new GlobalUsageRights(null, null)));
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDocument command.", e);
                throw;
            }
            
            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Getting, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

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

        private void cmd_OnComplete(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Commands.ReplyBase reply)
        {
            if (!GetGlobalPermissions_OnComplete_IsAuthorized(_request, reply, Security.Authorization.GlobalPermissionType.GetGlobalPermissions))
                return;

            // Authorized
            _onComplete(_request, reply);
        }

        private void cmd_OnTimeout(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection)
        {
            _onTimeout(_request);
        }

        private void cmd_OnProgress(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (_onProgress != null) _onProgress(_request, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

        private void cmd_OnError(Commands.Base sender, Networking.Http.Client client, string message, System.Exception exception)
        {
            _onError(_request, message, exception);
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Will never be called
            throw new System.NotImplementedException();
        }
    }
}
