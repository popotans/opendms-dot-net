using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class CreateNewResource : Base
    {
        private Data.Metadata _resourceMetadata = null;
        private Data.Metadata _versionMetadata = null;
        private Data.Content _versionContent = null;
        private Data.Resource _resource = null;
        private Data.Version _version = null;
        private Transactions.Transaction _tResource = null;
        private Transactions.Transaction _tVersion = null;

        public CreateNewResource(EngineRequest request, Data.Metadata resourceMetadata, Data.Metadata versionMetadata, Data.Content versionContent)
            : base(request)
        {
            _resourceMetadata = resourceMetadata;
            _versionMetadata = versionMetadata;
            _versionContent = versionContent;
        }

        public override void Execute()
        {
            // Get the resourceusagerightstemplate
            EngineRequest request = new EngineRequest();
            request.AuthToken = _request.AuthToken;
            request.Database = _request.Database;
            request.Engine = _request.Engine;
            request.OnComplete += new EngineBase.CompletionDelegate(RURT_Complete);
            request.OnError += new EngineBase.ErrorDelegate(RURT_Error);
            request.OnProgress += new EngineBase.ProgressDelegate(RURT_Progress);
            request.OnTimeout += new EngineBase.TimeoutDelegate(RURT_Timeout);
            request.RequestingPartyType = Security.RequestingPartyType.System;

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.GettingResourceUsageRightsTemplate, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            _request.Engine.GetResourceUsageRightsTemplate(request);
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Never called as this is a new resource
            throw new NotImplementedException();
        }

        private void RURT_Error(EngineRequest request, string message, Exception exception)
        {
            _onError(_request, message, exception);
        }

        private void RURT_Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            _onProgress(_request, direction, packetSize, sendPercentComplete, receivePercentComplete);
        }

        private void RURT_Timeout(EngineRequest request)
        {
            _onTimeout(_request);
        }

        private void RURT_Complete(EngineRequest request, ICommandReply reply)
        {
            Data.ResourceId rid;
            Data.VersionId vid;
            List<Data.VersionId> versionList;
            Commands.GetDocumentReply r;
            Transitions.ResourceUsageRights txRights;
            CouchDB.ResourceUsageRightsTemplate rurt;
            string creatingUsername;
            Transactions.Stage stage;
            Transactions.Actions.CreateNewResource action;
            Model.Document doc;
            Transitions.Resource txResource;
            List<Exception> errors;

            rid = Data.ResourceId.Create();
            vid = new Data.VersionId(rid, 0);
            versionList = new List<Data.VersionId>();
            
            r = (Commands.GetDocumentReply)reply;
            
            if (r.IsError)
            {
                _onError(_request, "An error occurred while attempting to get the resource permissions template, the message is: " + r.ErrorMessage, null);
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

            if(request.RequestingPartyType == Security.RequestingPartyType.System)
                creatingUsername = "System";
            else
                creatingUsername = request.Session.User.Username;

            txRights = new Transitions.ResourceUsageRights();
            rurt = (ResourceUsageRightsTemplate)txRights.Transition(r.Document);
            _version = new Data.Version(vid, _versionMetadata, _versionContent);
            versionList.Add(vid);
            _resource = new Data.Resource(rid, null, versionList, vid, _resourceMetadata, rurt.UsageRights);
            _tResource = Transactions.Manager.Instance.CreateTransaction(creatingUsername, _resource.ResourceId.ToString());
            stage = _tResource.Begin(creatingUsername, new System.TimeSpan(0, 5, 0));
            txResource = new Transitions.Resource();
            doc = txResource.Transition(_resource, out errors);

            if (errors != null && errors.Count > 0)
            {
                _tResource.Abort("System");
                _onError(_request, errors[0].Message, errors[0]);
            }

            // Add the "CreationFailedFlag" and set it to true
            doc["CreationFailedFlag"] = true;

            action = new Transactions.Actions.CreateNewResource(_request.Database, doc);

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CreatingNewResource, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            // Executes and saves locally
            stage.Execute(action);

            _tResource.OnCommitError += new Transactions.Transaction.CommitErrorDelegate(CommitResource_OnCommitError);
            _tResource.OnCommitTimeout += new Transactions.Transaction.CommitTimeoutDelegate(CommitResource_OnCommitTimeout);
            _tResource.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(CommitResource_OnCommitProgress);
            _tResource.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(CommitResource_OnCommitSuccess);

            // Sends to CouchDB
            _tResource.Commit(stage, creatingUsername, action);
        }

        private void CommitResource_OnCommitError(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
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

            _tResource.Abort("System");
        }

        private void CommitResource_OnCommitTimeout(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
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

            _tResource.Abort("System");
        }

        private void CommitResource_OnCommitProgress(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                throw;
            }
        }

        private void CommitResource_OnCommitSuccess(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, ICommandReply reply)
        {
            Transactions.Stage vStage;
            Transitions.Version txVersion;
            Model.Document doc;
            string creatingUsername;
            List<Exception> errors;
            Commands.PutDocumentReply putDocReply;

            // Unsubscribe
            _tResource.OnCommitError -= CommitResource_OnCommitError;
            _tResource.OnCommitTimeout -= CommitResource_OnCommitTimeout;
            _tResource.OnCommitProgress -= CommitResource_OnCommitProgress;
            _tResource.OnCommitSuccess -= CommitResource_OnCommitSuccess;


            // We need to update the resource so that we can later access it to remove the CreationFailedFlag
            putDocReply = (Commands.PutDocumentReply)reply;
            _resource.UpdateRevision(putDocReply.Rev);

            if (_request.RequestingPartyType == Security.RequestingPartyType.System)
                creatingUsername = "System";
            else
                creatingUsername = _request.Session.User.Username;

            _tVersion = Transactions.Manager.Instance.CreateTransaction(creatingUsername, _version.VersionId.ToString());
            vStage = _tVersion.Begin(creatingUsername, new TimeSpan(0, 5, 0));
            txVersion = new Transitions.Version();
            doc = txVersion.Transition(_version, out errors);

            if (errors != null && errors.Count > 0)
            {
                _tResource.Abort("System");
                _onError(_request, errors[0].Message, errors[0]);
            }

            action = new Transactions.Actions.CreateNewVersion(_request.Database, doc);

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.CreatingNewVersion, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            // Executes and saves locally
            vStage.Execute(action);

            _tVersion.OnCommitError += new Transactions.Transaction.CommitErrorDelegate(CommitVersion_OnCommitError);
            _tVersion.OnCommitTimeout += new Transactions.Transaction.CommitTimeoutDelegate(CommitVersion_OnCommitTimeout);
            _tVersion.OnCommitProgress += new Transactions.Transaction.CommitProgressDelegate(CommitVersion_OnCommitProgress);
            _tVersion.OnCommitSuccess += new Transactions.Transaction.CommitSuccessDelegate(CommitVersion_OnCommitSuccess);

            // Sends to CouchDB
            _tVersion.Commit(vStage, creatingUsername, action);
        }

        private void CommitVersion_OnCommitError(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
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
            
            _tResource.Abort("System");
            _tVersion.Abort("System");
            DeleteResource(_resource.ResourceId);
        }

        private void CommitVersion_OnCommitTimeout(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, string message, Exception exception)
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

            _tResource.Abort("System");
            _tVersion.Abort("System");
            DeleteResource(_resource.ResourceId);
        }

        private void CommitVersion_OnCommitProgress(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            try { _onProgress(_request, Networking.Http.DirectionType.Upload, packetSize, sendPercentComplete, receivePercentComplete); }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                throw;
            }
        }

        private void CommitVersion_OnCommitSuccess(Transactions.Transaction sender, Transactions.Stage stage, Transactions.Actions.Base action, ICommandReply reply)
        {
            // Now, we need to remove the CreationFailedFlag
            EngineRequest request = new EngineRequest();
            request.AuthToken = _request.AuthToken;
            request.Database = _request.Database;
            request.Engine = _request.Engine;
            request.OnComplete = (request2, reply2) =>
                {
                    try { _onComplete(_request, null); }
                    catch (Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the method specified in the _onComplete argument.", e);
                        throw;
                    }
                };
            request.OnError = (request2, message, exception) =>
                {
                    try { _onError(_request, "An exception occurred while attempting to delete the resource.", exception); }
                    catch (Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                        throw;
                    }
                };
            request.OnProgress = (request2, direction, packetSize, sendPercentComplete, receivePercentComplete) =>
                {
                    try { _onProgress(_request, direction, packetSize, sendPercentComplete, receivePercentComplete); }
                    catch (Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the method specified in the onProgress argument.", e);
                        throw;
                    }
                };
            request.OnTimeout = (request2) =>
                {
                    try { _onTimeout(_request); }
                    catch (Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
                        throw;
                    }
                };
            request.RequestingPartyType = Security.RequestingPartyType.System;


            // We do not need to notify the engine about the flag because Data.Resource itself does not understand the flag, so it is not saved in the resource it only applied to the transition.
            request.Engine.ModifyResource(request, _resource);
        }


        private void DeleteResource(Data.ResourceId resourceId)
        {
            EngineRequest request = new EngineRequest();
            request.AuthToken = _request.AuthToken;
            request.Database = _request.Database;
            request.Engine = _request.Engine;
            // We do not need completion as the event would have already been pumped to the subscriber as an error or timeout before this method was called
            // request.OnComplete;
            request.OnError = (request2, message, exception) =>
                {
                    try { _onError(_request, "An exception occurred while attempting to delete the resource.", exception); }
                    catch (Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the method specified in the onError argument.", e);
                        throw;
                    }
                };
            // We do not want to pump a progress event
            // request.OnProgress += new EngineBase.ProgressDelegate(DeleteResource_Progress);
            request.OnTimeout = (request2) =>
                {
                    try { _onTimeout(_request); }
                    catch (Exception e)
                    {
                        Logger.Storage.Error("An exception occurred while calling the method specified in the onTimeout argument.", e);
                        throw;
                    }
                };
            request.RequestingPartyType = Security.RequestingPartyType.System;

            _request.Engine.DeleteResource(request, resourceId);
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            throw new NotImplementedException();
        }
    }
}
