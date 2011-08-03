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
        private Dictionary<IDatabase, Security.DatabaseSessionManager> _dsms;

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
            ((Providers.EngineBase)_request.Engine).RegisterOnInitialized(_onInitialized);
            _ignoringInitializationComplete = false;
            _request.Engine.SetState(true, false);
            _dsms = new Dictionary<IDatabase, Security.DatabaseSessionManager>();

            for (int i = 0; i < _databases.Count; i++)
            {
                Transactions.Transaction t;
                Transactions.Processes.GetAllGroups process;

                process = new Transactions.Processes.GetAllGroups(_databases[i], 
                    _request.RequestingPartyType, _request.Session);
                t = new Transactions.Transaction(process);
                t.OnError += delegate(Transactions.Transaction sender, Transactions.Processes.Base process2, Transactions.Tasks.Base task, string message, Exception exception)
                {
                    Logger.Storage.Error("An error occurred while running GetAllGroups on Database '" + _databases[i].Name + "', message: " + message, exception);
                    ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(false, message, exception);
                };
                t.OnTimeout += delegate(Transactions.Transaction sender, Transactions.Processes.Base process2, Transactions.Tasks.Base task)
                {
                    Logger.Storage.Error("A timeout occurred while running GetAllGroups on Database '" + _databases[i].Name + "'.");
                    ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(false, "Timeout", null);                    
                };
                t.OnComplete += delegate(Transactions.Transaction sender, Transactions.Processes.Base process2)
                {
                    lock (_dsms)
                    {
                        _dsms.Add(_request.Database, new Security.DatabaseSessionManager(_request.Engine,
                            _request.Database, ((Transactions.Processes.GetAllGroups)process2).Groups));
                        if (_dsms.Count == _databases.Count)
                        {
                            _request.Engine.SetState(false, true);
                            Logger.Storage.Debug("Engine initialized.");
                            _sessionMgr.Initialize(_request.Engine, _dsms);
                            ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(true, "Initialization successful.", null);
                        }
                    }
                    Logger.Storage.Debug("All groups for the database named " + _databases[i].Name + " have been loaded.");
                };
                t.Execute();
            }
        }
    }
}
