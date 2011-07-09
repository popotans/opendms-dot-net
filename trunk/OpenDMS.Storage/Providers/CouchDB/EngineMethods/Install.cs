using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class Install : Base
    {
        // Installation package, administrator user password is 'password'.
        private string _package =
@"{ 
    ""docs"" : [
        {
            ""_id"" : ""_design/groups"",
            ""language"": ""javascript"",
            ""views"": {
                ""GetAll"": {
                    ""map"": ""function(doc) { if (doc.Type == \""group\"") { emit(doc); } }""
                }
            }
        },
        {
            ""_id"": ""_design/users"",
            ""language"": ""javascript"",
            ""views"": {
                ""GetAll"": {
                    ""map"": ""function(doc) { if (doc.Type == \""user\"") { emit(doc); } }""
                }
            }
        },
        {
            ""_id"": ""globalusagerights"",
            ""Type"": ""globalusagerights"",
            ""UsageRights"": [
                {
                    ""group-administrators"": 1118481
                }
            ]
        },
        {
            ""_id"": ""group-administrators"",
            ""Type"": ""group"",
            ""Groups"": [
            ],
            ""Users"": [
                ""user-administrator""
            ]
        },
        {
            ""_id"": ""user-administrator"",
            ""Type"": ""user"",
            ""Password"": ""sQnzu7wkTrgkQZF+0G1hi5AI3Qmzvv0bXgc5THBqi7mAsdd4Xll27ASbRt9fEyavWi6m0QP9B8lThf+rDKy8hg=="",
            ""FirstName"": null,
            ""MiddleName"": null,
            ""LastName"": null,
            ""Superuser"": true,
            ""Groups"": [
                ""group-administrators""
            ]
        }
    ]
}";

        public Install(EngineRequest request)
            : base(request)
        {
        }

        public override void Execute()
        {
            Model.BulkDocuments package;
            Commands.PostBulkDocuments cmd;

            if (_request.RequestingPartyType != Security.RequestingPartyType.System)
            {
                // Not authorized
                if (_onAuthorizationDenied != null) _onAuthorizationDenied(_request);
                else throw new NotImplementedException("OnAuthorizationDenied must have a subscriber.");
                return;
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

            //Newtonsoft.Json.Linq.JObject jobj = Newtonsoft.Json.Linq.JObject.Parse(_package);
            package = new Model.BulkDocuments(_package);
            package.AllOrNothing = true;
            cmd = new Commands.PostBulkDocuments(_request.Database, package);

            AttachSubscriberEvent(cmd, _request.OnProgress);
            AttachSubscriberEvent(cmd, _request.OnComplete);
            AttachSubscriberEvent(cmd, _request.OnError);
            AttachSubscriberEvent(cmd, _request.OnTimeout);
            
            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Installing, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            cmd.Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Never called
            throw new NotImplementedException();
        }

        #region The Old Way - Non-Batch Based

        //public Install(EngineRequest request)
        //    : base(request)
        //{
        //    List<string> usersOfAdministratorsGroup = new List<string>();
        //    List<string> groupsOfAdministratorUser = new List<string>();
        //    List<Security.UsageRight> groupUsageRights = new List<Security.UsageRight>();

        //    usersOfAdministratorsGroup.Add("administrator");
        //    groupsOfAdministratorUser.Add("administrators");

        //    _adminGroup = new Security.Group("administrators", null, usersOfAdministratorsGroup, null);
        //    _adminUser = new Security.User("administrator", null, "password", null, null, null, groupsOfAdministratorUser, true);

        //    Security.UsageRight groupUr = new Security.UsageRight(_adminGroup, Security.Authorization.GlobalPermissionType.All);
        //    Security.UsageRight userUr = new Security.UsageRight(_adminUser, Security.Authorization.GlobalPermissionType.All);
        //    groupUsageRights.Add(groupUr);
        //    groupUsageRights.Add(userUr);

        //    _gur = new GlobalUsageRights(null, groupUsageRights);
        //}

        //public override void Execute()
        //{
        //    Transactions.Stage stage;
        //    Transitions.GlobalUsageRights txGur;
        //    Model.Document doc;
        //    Transactions.Actions.CreateGlobalUsageRights action;

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    // Delete all transactions
        //    Transactions.Manager.Instance.AbortAllTransactions();

        //    // Create GlobalUsageRights
        //    _t = Transactions.Manager.Instance.CreateTransaction("System", _adminGroup.Id);
        //    stage = _t.Begin("System", new System.TimeSpan(0, 5, 0));
        //    txGur = new Transitions.GlobalUsageRights();
        //    doc = txGur.Transition(_gur);
        //    action = new Transactions.Actions.CreateGlobalUsageRights(_request.Database, doc);

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CreatingGlobalUsageRights, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    // Executes and saves locally
        //    stage.Execute(action);

        //    _t.OnCommitError += new Transactions.Transaction.CommitErrorDelegate(CreateGUR_OnCommitError);
        //    _t.OnCommitTimeout += new Transactions.Transaction.CommitTimeoutDelegate(CreateGUR_OnCommitTimeout);
        //    _t.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(CreateGUR_OnCommitProgress);
        //    _t.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(CreateGUR_OnCommitSuccess);

        //    // Sends to CouchDB
        //    _t.Commit(stage, "System", action);
        //}

        //protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        //{
        //    // Never called
        //    throw new NotImplementedException();
        //}

        //private void CreateGUR_OnCommitSuccess(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, ICommandReply reply)
        //{
        //    Transitions.Group txGroup;
        //    Model.Document doc;
        //    Transactions.Actions.CreateGroup act;

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    // Create Administrators Group

        //    _t = Transactions.Manager.Instance.CreateTransaction("System", _adminGroup.Id);
        //    stage = _t.Begin("System", new System.TimeSpan(0, 5, 0));
        //    txGroup = new Transitions.Group();
        //    doc = txGroup.Transition(_adminGroup);
        //    act = new Transactions.Actions.CreateGroup(_request.Database, doc);

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CreatingGroup, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    // Executes and saves locally
        //    stage.Execute(act);

        //    _t.OnCommitError += new Transactions.Transaction.CommitErrorDelegate(CreateGroup_OnCommitError);
        //    _t.OnCommitTimeout += new Transactions.Transaction.CommitTimeoutDelegate(CreateGroup_OnCommitTimeout);
        //    _t.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(CreateGroup_OnCommitProgress);
        //    _t.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(CreateGroup_OnCommitSuccess);

        //    // Sends to CouchDB
        //    _t.Commit(stage, "System", act);
        //}

        //private void CreateGUR_OnCommitProgress(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        //{
        //    try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
        //        throw;
        //    }
        //}

        //private void CreateGUR_OnCommitError(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        //{
        //    // Abort after subscriber has a chance to get any info.
        //    try { _onError(_request, message, exception); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
        //        throw;
        //    }

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    _t.Abort("System");
        //}

        //private void CreateGUR_OnCommitTimeout(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        //{
        //    // Abort after subscriber has a chance to get any info.
        //    try { _onTimeout(_request); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
        //        throw;
        //    }

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    _t.Abort("System");
        //}
        
        //private void CreateGroup_OnCommitSuccess(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, ICommandReply reply)
        //{
        //    Transitions.User txUser;
        //    Model.Document doc;
        //    Transactions.Actions.CreateUser act;

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    // Create Administrators Group

        //    _t = Transactions.Manager.Instance.CreateTransaction("System", _adminGroup.Id);
        //    stage = _t.Begin("System", new System.TimeSpan(0, 5, 0));
        //    txUser = new Transitions.User();
        //    doc = txUser.Transition(_adminUser);
        //    act = new Transactions.Actions.CreateUser(_request.Database, doc);

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CreatingUser, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    // Executes and saves locally
        //    stage.Execute(action);

        //    _t.OnCommitError += new Transactions.Transaction.CommitErrorDelegate(CreateUser_OnCommitError);
        //    _t.OnCommitTimeout += new Transactions.Transaction.CommitTimeoutDelegate(CreateUser_OnCommitTimeout);
        //    _t.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(CreateUser_OnCommitProgress);
        //    _t.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(CreateUser_OnCommitSuccess);

        //    // Sends to CouchDB
        //    _t.Commit(stage, "System", act);
        //}

        //private void CreateGroup_OnCommitProgress(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        //{
        //    try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
        //        throw;
        //    }
        //}

        //private void CreateGroup_OnCommitError(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        //{
        //    // Abort after subscriber has a chance to get any info.
        //    try { _onError(_request, message, exception); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
        //        throw;
        //    }

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    _t.Abort("System");
        //}

        //private void CreateGroup_OnCommitTimeout(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        //{
        //    // Abort after subscriber has a chance to get any info.
        //    try { _onTimeout(_request); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
        //        throw;
        //    }

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    _t.Abort("System");
        //}

        //private void CreateUser_OnCommitSuccess(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, ICommandReply reply)
        //{
        //    // We return a null because we did a lot, a single result would be misleading.
        //    try { _onComplete(_request, null); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onComplete argument.", e);
        //        throw;
        //    }
        //}

        //private void CreateUser_OnCommitProgress(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        //{
        //    try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
        //        throw;
        //    }
        //}

        //private void CreateUser_OnCommitError(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        //{
        //    // Abort after subscriber has a chance to get any info.
        //    try { _onError(_request, message, exception); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
        //        throw;
        //    }

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    _t.Abort("System");
        //}

        //private void CreateUser_OnCommitTimeout(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
        //{
        //    // Abort after subscriber has a chance to get any info.
        //    try { _onTimeout(_request); }
        //    catch (Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
        //        throw;
        //    }

        //    try
        //    {
        //        if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Aborting, true);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
        //        throw;
        //    }

        //    _t.Abort("System");
        //}

        #endregion
    }
}
