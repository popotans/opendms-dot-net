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
                    _request.RequestingPartyType, Security.Session.MakeSecurityOverride(), 15000,
                    15000, 8194, 8194);
                t = new Transactions.Transaction(process);
                
                AttachSubscriber(process, _request.OnActionChanged);
                AttachSubscriber(process, _request.OnAuthorizationDenied);
                AttachSubscriber(process, _request.OnComplete);
                AttachSubscriber(process, _request.OnError);
                AttachSubscriber(process, _request.OnProgress);
                AttachSubscriber(process, _request.OnTimeout);

                //process.OnActionChanged += delegate(Transactions.Processes.Base sender, Transactions.Tasks.Base task, EngineActionType actionType, bool willSendProgress)
                //{
                //};
                process.OnAuthorizationDenied += delegate(Transactions.Processes.Base sender, Transactions.Tasks.Base task)
                {
                    Logger.Storage.Error("Authorization failed while running GetAllGroups on Database '" + sender.Database.Name + "'.");
                    ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(false, "Authorization failed.", null);
                };
                process.OnComplete += delegate(Transactions.Processes.Base sender, ICommandReply reply, object result)
                {
                    lock (_dsms)
                    {
                        _dsms.Add(sender.Database, new Security.DatabaseSessionManager(_request.Engine,
                            sender.Database, ((Transactions.Processes.GetAllGroups)sender).Groups));
                        if (_dsms.Count == _databases.Count)
                        {
                            _request.Engine.SetState(false, true);
                            Logger.Storage.Debug("Engine initialized.");
                            _sessionMgr.Initialize(_request.Engine, _dsms);
                            ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(true, "Initialization successful.", null);
                        }
                    }
                    Logger.Storage.Debug("All groups for the database named " + sender.Database.Name + " have been loaded.");
                };
                process.OnError += delegate(Transactions.Processes.Base sender, Transactions.Tasks.Base task, string message, Exception exception)
                {
                    Logger.Storage.Error("An error occurred while running GetAllGroups on Database '" + sender.Database.Name + "', message: " + message, exception);
                    ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(false, message, exception);
                };
                //process.OnProgress += delegate(Transactions.Processes.Base sender, Transactions.Tasks.Base task, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
                //{
                //};
                //process.OnTaskComplete += delegate(Transactions.Processes.Base sender, Transactions.Tasks.Base task)
                //{
                //};
                process.OnTimeout += delegate(Transactions.Processes.Base sender, Transactions.Tasks.Base task)
                {
                    Logger.Storage.Error("A timeout occurred while running GetAllGroups on Database '" + sender.Database.Name + "'.");
                    ((Providers.EngineBase)_request.Engine).TriggerOnInitialized(false, "Timeout", null);             
                };

                t.Execute();
            }
        }
    }
}
