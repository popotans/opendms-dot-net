using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class ModifyResource : Base
    {
        private Transactions.Transaction _t;
        private Data.Resource _resource = null;

        public ModifyResource(EngineRequest request, Data.Resource resource)
            : base(request)
        {
            _resource = resource;
        }

        public override void Execute()
        {
            GetResourcePermissions(_resource.ResourceId);
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            Transactions.Stage stage;
            Transitions.Resource txResource;
            Model.Document doc;
            string username;
            List<Exception> errors;
            Transactions.Actions.ModifyResource action;

            // Check permissions
            if (!GetResourcePermissions_OnComplete_IsAuthorized(request, reply, Security.Authorization.ResourcePermissionType.Modify))
                return;

            if (request.RequestingPartyType == Security.RequestingPartyType.System)
                username = "System";
            else
                username = request.Session.User.Username;

            _t = Transactions.Manager.Instance.CreateTransaction(username, _resource.ResourceId.ToString());
            stage = _t.Begin(username, new TimeSpan(0, 5, 0));
            txResource = new Transitions.Resource();
            doc = txResource.Transition(_resource, out errors);
            action = new Transactions.Actions.ModifyResource(request.Database, doc);

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.ModifyingResource, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            // Executes and saves locally
            stage.Execute(action);

            _t.OnCommitError += new Transactions.Transaction.CommitErrorDelegate(Commit_OnCommitError);
            _t.OnCommitTimeout += new Transactions.Transaction.CommitTimeoutDelegate(Commit_OnCommitTimeout);
            _t.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(Commit_OnCommitProgress);
            _t.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(Commit_OnCommitSuccess);

            // Sends to CouchDB
            _t.Commit(stage, username, action);
        }

        private void Commit_OnCommitError(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        {
            // Abort after subscriber has a chance to get any info.
            try { _onError(_request, message, exception); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                throw;
            }

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            _t.Abort("System");
        }

        private void Commit_OnCommitTimeout(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        {
            // Abort after subscriber has a chance to get any info.
            try { _onTimeout(_request); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                throw;
            }

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            _t.Abort("System");
        }

        private void Commit_OnCommitSuccess(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, ICommandReply reply)
        {
            try { _onComplete(_request, reply); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onComplete argument.", e);
                throw;
            }
        }

        private void Commit_OnCommitProgress(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                throw;
            }
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            throw new NotImplementedException();
        }
    }
}
