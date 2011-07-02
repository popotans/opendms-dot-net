using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public abstract class Base
    {
		#region Fields (7) 

        protected bool _isEventSubscriptionSuppressed = false;
        protected Engine.ActionDelegate _onActionChanged = null;
        protected Engine.CompletionDelegate _onComplete = null;
        protected Engine.ErrorDelegate _onError = null;
        protected Engine.ProgressDelegate _onProgress = null;
        protected Engine.TimeoutDelegate _onTimeout = null;
        protected Engine.AuthorizationDelegate _onAuthorizationDenied = null;
        protected EngineRequest _request;

		#endregion Fields 

		#region Constructors (1) 

        public Base(EngineRequest request)
        {
            if (request == null) return;

            _request = request;
            _onActionChanged = request.OnActionChanged;
            _onProgress = request.OnProgress;
            _onComplete = request.OnComplete;
            _onTimeout = request.OnTimeout;
            _onError = request.OnError;
            _onAuthorizationDenied = request.OnAuthorizationDenied;
        }

		#endregion Constructors 

		#region Methods (5) 

		// Public Methods (1) 

        public abstract void Execute();
		// Protected Methods (4) 

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.TimeoutDelegate onTimeout)
        {
            Commands.Base.TimeoutDelegate timeoutDelegate = (sender, client, connection) =>
            {
                try
                {
                    if (onTimeout != null &&
                        !_isEventSubscriptionSuppressed)
                        onTimeout(_request);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
                    throw;
                }
            };
            cmd.OnTimeout += timeoutDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.ErrorDelegate onError)
        {
            Commands.Base.ErrorDelegate errorDelegate = (sender, client, message, exception) =>
            {
                try
                {
                    if (onError != null &&
                        !_isEventSubscriptionSuppressed)
                        onError(_request, message, exception);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                    throw;
                }
            };
            cmd.OnError += errorDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.CompletionDelegate onComplete)
        {
            Commands.Base.CompletionDelegate completionDelegate = (sender, client, connection, reply) =>
            {
                try
                {
                    if (onComplete != null &&
                        !_isEventSubscriptionSuppressed)
                        onComplete(_request, reply);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onComplete argument.", e);
                    throw;
                }
            };
            cmd.OnComplete += completionDelegate;
        }

        protected void AttachSubscriberEvent(Commands.Base cmd, Engine.ProgressDelegate onProgress)
        {
            Commands.Base.ProgressDelegate progressDelegate = (sender, client, connection, direction, packetSize, sendPercentComplete, receivePercentComplete) =>
            {
                try
                {
                    if (onProgress != null &&
                        !_isEventSubscriptionSuppressed)
                        onProgress(_request, direction, packetSize, sendPercentComplete, receivePercentComplete);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                    throw;
                }
            };
            cmd.OnProgress += progressDelegate;
        }
        
        protected void GetGlobalPermissions()
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

            EngineRequest request = new EngineRequest();
            request.Database = _request.Database;
            request.Engine = _request.Engine;
            request.OnComplete += new EngineBase.CompletionDelegate(GetGlobalPermissions_OnComplete);
            request.OnError += new EngineBase.ErrorDelegate(GetGlobalPermissions_OnError);
            request.OnTimeout += new EngineBase.TimeoutDelegate(GetGlobalPermissions_OnTimeout);
            request.OnProgress += new EngineBase.ProgressDelegate(GetGobalPermissions_OnProgress);
            request.RequestingPartyType = Security.RequestingPartyType.System;

            request.Engine.GetGlobalPermissions(request);
        }

        protected abstract void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply);

        protected virtual bool GetGlobalPermissions_OnComplete_IsAuthorized(EngineRequest request, ICommandReply reply, Security.Authorization.GlobalPermissionType requiredPermissions)
        {
            Commands.GetDocumentReply r;
            Transitions.GlobalUsageRights txGur;
            GlobalUsageRights gur;

            r = (Commands.GetDocumentReply)reply;

            if (r.IsError)
            {
                _onError(_request, "An error occurred while attempting to get the global permissions, the message is: " + r.ErrorMessage, null);
                return false;
            }

            txGur = new Transitions.GlobalUsageRights();
            gur = (GlobalUsageRights)txGur.Transition(r.Document);

            if (_request.RequestingPartyType != Security.RequestingPartyType.System)
            {
                if (!Security.Authorization.Manager.IsAuthorized(gur.UsageRights,
                    requiredPermissions,
                    _request.GetGroupMembership(), request.Session.User.Username))
                {
                    // Not authorized
                    if (_onAuthorizationDenied != null) _onAuthorizationDenied(_request);
                    else throw new NotImplementedException("OnAuthorizationDenied must have a subscriber.");
                    return false;
                }
            }

            // Authorized
            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, false);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            return true;
        }

        protected virtual void GetGlobalPermissions_OnTimeout(EngineRequest request)
        {
            _onTimeout(request);
        }

        protected virtual void GetGlobalPermissions_OnError(EngineRequest request, string message, Exception exception)
        {
            _onError(request, message, exception);
        }

        protected virtual void GetGobalPermissions_OnProgress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (_onProgress != null) _onProgress(request, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

		#endregion Methods 
    }
}
