using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateUser : Base
    {
        private IDatabase _db = null;
        private Security.User _user = null;

        public CreateUser(EngineRequest request, Security.SessionManager sessionManager, IDatabase db, Security.User user)
            : base(request, sessionManager)
        {
        }

        public override void Execute()
        {
            Transactions.Transaction t;
            Transactions.Stage stage;
            Transitions.User txUser;
            Model.Document doc;
            Transactions.Actions.CreateUser action;

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
                Security.Session session = _sessionManager.LookupSession(_db, _request.AuthToken);
                if (session == null)
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
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, false);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            t = Transactions.Manager.Instance.CreateTransaction("System", _user.Id);
            stage = t.Begin("System", new System.TimeSpan(0, 5, 0));
            txUser = new Transitions.User();
            doc = txUser.Transition(_user);
            action = new Transactions.Actions.CreateUser(_db, doc);

            stage.OnCommitFailure += new Transactions.Stage.CommitFailureDelegate(Commit_OnCommitFailure);
            stage.OnCommitProgress += new Transactions.Stage.CommitProgressDelegate(Commit_OnCommitProgress);
            stage.OnCommitSuccess += new Transactions.Stage.CommitSuccessDelegate(Commit_OnCommitSuccess);
            stage.Execute(new Transactions.Actions.CreateUser(_db, doc));

            stage.Commit(action);
        }

        private void Commit_OnCommitSuccess(Transactions.Stage sender, Transactions.Actions.Base action, ICommandReply reply)
        {
            sender.
            // Check authorization
            try { _onComplete(_request, reply); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onComplete argument.", e);
                throw;
            }
        }

        private void Commit_OnCommitProgress(Transactions.Stage sender, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                throw;
            }
        }

        private void Commit_OnCommitFailure(Transactions.Stage sender, Transactions.Actions.Base action, string message, System.Exception exception)
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
