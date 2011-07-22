using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class Initialize : Base
    {
        private Security.SessionManager _sessionMgr;
        private Providers.EngineBase.InitializationDelegate _onInitialized;
        private bool _ignoringInitializationComplete;
        private List<Providers.IDatabase> _databases;

        public Initialize(EngineRequest request, Security.SessionManager sessionManager,
            Providers.EngineBase.InitializationDelegate onInitialized, List<Providers.IDatabase> databases)
            : base(request)
        {
            _sessionMgr = sessionManager;
            _ignoringInitializationComplete = false;
            request.Engine.SetState(false, false);
            _databases = databases;
            _onInitialized = onInitialized;
        }

        public override void Execute()
        {
            Logger.Storage.Debug("Initializing engine...");
            _sessionMgr.OnError += new Security.SessionManager.ErrorDelegate(Initialize_OnError);
            _sessionMgr.OnInitializationComplete += new Security.SessionManager.InitializationCompletionDelegate(Initialize_OnInitializationComplete);
            ((Providers.EngineBase)_request.Engine).RegisterOnInitialized(_onInitialized);
            _ignoringInitializationComplete = false;
            _request.Engine.SetState(true, false);
            _sessionMgr.Initialize(_request.Engine, _databases);
        }

        private void Initialize_OnError(string message, Exception exception)
        {
            _ignoringInitializationComplete = true;

            _sessionMgr.OnError -= Initialize_OnError;
            _sessionMgr.OnInitializationComplete -= Initialize_OnInitializationComplete;

            _request.Engine.SetState(false, false);
            Logger.Storage.Error("An error occurred while trying to initialize the engine.", exception);
            ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(false, message, exception);
        }

        private void Initialize_OnInitializationComplete()
        {
            _sessionMgr.OnError -= Initialize_OnError;
            _sessionMgr.OnInitializationComplete -= Initialize_OnInitializationComplete;

            if (_ignoringInitializationComplete) return;

            _request.Engine.SetState(false, true);
            Logger.Storage.Debug("Engine initialized.");
            ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(true, "Initialization successful.", null);
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Not called
            throw new NotImplementedException();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Not called
            throw new System.NotImplementedException();
        }
    }
}
