using System;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.TransactionsOld.Actions
{
    public class GetResource : Base
    {
        private IEngine _engine;
        private Guid _authToken;
        private Data.ResourceId _resourceId;
        private Manager _tManager;
        private string _requestingUsername;
        private EngineActionType _currentEngineAction;
        private Commands.GetDocumentReply _reply;

        public GetResource(IDatabase db, IEngine engine, Guid authToken, Data.ResourceId resourceId, 
            string requestingUsername)
            : base(db, null)
        {
            _engine = engine;
            _tManager = Transactions.Manager.Instance;
            _authToken = authToken;
            _resourceId = resourceId;
            _requestingUsername = requestingUsername;
            _currentEngineAction = EngineActionType.None;
        }

        public override Model.Document Execute()
        {
            return null;
        }

        public override void Commit()
        {
            Lock currentLock;

            // Request a local lock
            if (!_tManager.GetLocalResourceLock(_resourceId.ToString(), _requestingUsername, new TimeSpan(0, 1, 0), out currentLock))
            {
                TriggerOnAccessDenied(AccessDeniedReasonEnum.ExistingLock, currentLock.OwningUsername);
                return;
            } 

            // Download Resource
            // We want to use the engine because it will do the permissions checking
            EngineRequest request = new EngineRequest();
            request.Database = _db;
            request.Engine = _engine;
            request.RequestingPartyType = Security.RequestingPartyType.User;
            request.AuthToken = _authToken;
            request.OnActionChanged = delegate(EngineRequest request2, EngineActionType actionType, bool willSendProgress)
            {
                _currentEngineAction = actionType;
            };
            request.OnAuthorizationDenied = delegate(EngineRequest request2)
            {
                TriggerOnAccessDenied(AccessDeniedReasonEnum.Permissions, "Access denied.");
            };
            request.OnComplete += new EngineBase.CompletionDelegate(Download_OnComplete);
            request.OnError = delegate(EngineRequest request2, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            request.OnProgress = delegate(EngineRequest request2, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                if (_currentEngineAction == EngineActionType.GettingResource)
                    TriggerOnProgress(1, 2, packetSize, sendPercentComplete, receivePercentComplete);
            };
            request.OnTimeout = delegate(EngineRequest request2)
            {
                TriggerOnTimeout();
            };

            _engine.GetResource(request, _resourceId, true);
        }

        private void Download_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Check lock
            Commands.PutDocument cmd;
            Newtonsoft.Json.Linq.JObject remainder;
            _reply = (Commands.GetDocumentReply)reply;
            Transitions.Resource txResource = new Transitions.Resource();
            Data.Resource resource = txResource.Transition(r.Document, out remainder);

            if (resource.CheckedOutTo != _requestingUsername)
            { // Another user has the resource
                TriggerOnAccessDenied(AccessDeniedReasonEnum.ExistingLock, resource.CheckedOutTo);
                return;
            }

            // If we are here, we are allowed to access the object.
            // We need to apply the lock in the database

            // Note, r.Document contains the original document received from CouchDB, all we 
            // need to do is insert the checkout information - this side-steps
            // the Data.Resource object to decrease the amount of work required.
            if (_reply.Document["CheckedOutTo"] == null)
                _reply.Document.Add(new Newtonsoft.Json.Linq.JProperty("CheckedOutTo", _requestingUsername));
            else
                _reply.Document["CheckedOutTo"] = _requestingUsername;

            if (_reply.Document["CheckedOutAt"] == null)
                _reply.Document.Add(new Newtonsoft.Json.Linq.JProperty("CheckedOutAt", DateTime.Now));
            else
                _reply.Document["CheckedOutAt"] = DateTime.Now;
            
            try
            {
                cmd = new Commands.PutDocument(_db, _reply.Document);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PutDocument command.", e);
                TriggerOnError("An exception occurred while creating the PutDocument command.", e);
                throw;
            }

            cmd.OnComplete += new Commands.Base.CompletionDelegate(ApplyLock_OnComplete);
            cmd.OnError += delegate(Commands.Base sender, OpenDMS.Networking.Http.Client client, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            cmd.OnProgress += delegate(Commands.Base sender, Client client, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                TriggerOnProgress(2, 2, packetSize, sendPercentComplete, receivePercentComplete);
            };
            cmd.OnTimeout += delegate(Commands.Base sender, Client client, Connection connection)
            {
                TriggerOnTimeout();
            };

            try
            {
                cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }

        private void ApplyLock_OnComplete(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Commands.ReplyBase reply)
        {
            try
            {
                _tManager.ReleaseLock(_resourceId.ToString());
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logger.Storage.Error("Failed to find a local lock for " + _resourceId.ToString() + " to release.", e);
            }

            TriggerOnComplete(_reply);
        }
    }
}
