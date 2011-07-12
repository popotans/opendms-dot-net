using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateUser : Base
    {
        private Security.User _user = null;
        private Transactions.Transaction _t;

        public CreateUser(EngineRequest request, Security.User user)
            : base(request)
        {
            _user = user;
        }

        public override void Execute()
        {
            GetGlobalPermissions();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            Transactions.Stage stage;
            Transitions.User txUser;
            Model.Document doc;
            Transactions.Actions.CreateUser action;
            string creatingUsername;

            if (!GetGlobalPermissions_OnComplete_IsAuthorized(request, reply, Security.Authorization.GlobalPermissionType.CreateUser))
                return;

            if(request.RequestingPartyType == Security.RequestingPartyType.System)
                creatingUsername = "System";
            else
                creatingUsername = request.Session.User.Username;
            
            _t = Transactions.Manager.Instance.CreateTransaction(creatingUsername, _user.Id);
            stage = _t.Begin(creatingUsername, new System.TimeSpan(0, 5, 0));
            txUser = new Transitions.User();
            doc = txUser.Transition(_user);
            action = new Transactions.Actions.CreateUser(request.Database, doc);

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CreatingUser, true);
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
            _t.Commit(stage, creatingUsername, action);
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

    }
}
