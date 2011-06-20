using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine : IEngine
    {
        public delegate void ActionDelegate(EngineActionType actionType, bool willSendProgress);
        public delegate void ProgressDelegate(DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void CompletionDelegate(ICommandReply reply);
        public delegate void TimeoutDelegate();
        public delegate void ErrorDelegate(string message, Exception exception);

        public Engine()
        {
        }

        #region Resource Actions

        public void CreateNewResource(IDatabase db, Metadata metadata, List<Security.UsageRight> usageRights, ActionDelegate onActionChanged, ProgressDelegate onProgress, CompletionDelegate onComplete, TimeoutDelegate onTimeout, ErrorDelegate onError)
        {
            EngineMethods.CreateNewResource act = new EngineMethods.CreateNewResource(db, metadata, usageRights, onActionChanged, onProgress, onComplete, onTimeout, onError);
            act.Execute();
        }

        public void GetCurrentVersion(Data.ResourceId id)
        {
        }

        public void GetResource(Data.ResourceId id)
        {
        }

        public void DeleteResource(Data.ResourceId id)
        {
        }

        public void Revert(Data.VersionId id)
        {
        }

        #endregion

        #region Version Actions

        public void CreateNewVersion(IDatabase db, Data.Version version, ActionDelegate onActionChanged, ProgressDelegate onProgress, CompletionDelegate onComplete, TimeoutDelegate onTimeout, ErrorDelegate onError)
        {
            List<Exception> errors = null;
            Model.Document doc;
            Commands.PutDocument cmd;

            // State check
            if (!version.CanCreateWithoutPropertiesOrContent &&
                !version.CanCreateWithoutPropertiesWithContent &&
                !version.CanCreateWithPropertiesAndContent &&
                !version.CanCreateWithPropertiesWithoutContent)
                throw new ArgumentException("Argument version cannot be created due to its current state.");

            doc = new Transitions.Version().Transition(version, out errors);

            if (errors != null && errors.Count > 0 && onError != null)
            {
                for (int i = 0; i < errors.Count; i++)
                    onError(errors[i].Message, errors[i]);
            }

            cmd = new Commands.PutDocument(db, doc);
            
            cmd.Execute(db.Server.Timeout, db.Server.Timeout, db.Server.BufferSize, db.Server.BufferSize);
        }

        private void CreateNewVersion_UpdateResource()
        {
        }

        public void GetCurrentVersion(Data.VersionId id)
        {
            GetCurrentVersion(id.ResourceId);
        }

        public HttpNetworkStream GetContentStream(Data.VersionId id)
        {
            return null;
        }

        #endregion

        #region Subscription Attachers for Commands


        #endregion
    }
}
