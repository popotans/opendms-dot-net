using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateUser : Base
    {
        private Security.User _user = null;

        public CreateUser(EngineRequest request, Security.User user)
            : base(request)
        {
        }

        public override void Execute()
        {
            GetGlobalPermissions();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            Transactions.Transaction t;
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

            t = Transactions.Manager.Instance.CreateTransaction(creatingUsername, _user.Id);
            stage = t.Begin(creatingUsername, new System.TimeSpan(0, 5, 0));
            txUser = new Transitions.User();
            doc = txUser.Transition(_user);
            action = new Transactions.Actions.CreateUser(request.Database, doc);

            // Executes and saves locally
            stage.Execute(new Transactions.Actions.CreateUser(request.Database, doc));

            t.OnCommitFailure += new Transactions.Transaction.CommitFailureDelegate(Commit_OnCommitFailure);
            t.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(Commit_OnCommitProgress);
            t.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(Commit_OnCommitSuccess);

            // Sends to CouchDB
            t.Commit(stage, creatingUsername, action);
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

        private void Commit_OnCommitFailure(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        {
            try { _onError(_request, message, exception); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                throw;
            }
        }
    }
}
