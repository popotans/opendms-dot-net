using System;
using System.Web;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading;
using IO = OpenDMS.IO;
using Storage = OpenDMS.Storage;
using Providers = OpenDMS.Storage.Providers;
using Requests = OpenDMS.Networking.Api.Requests;
using Responses = OpenDMS.Networking.Api.Responses;
using Newtonsoft.Json.Linq;

namespace OpenDMS.HttpModule
{
    public class ServiceHandler : IDisposable
    {
        private Storage.Providers.IEngine _engine;
        private Storage.Providers.IDatabase _db;
        private Storage.Providers.IServer _server;
        private bool _isInstalled;
        private bool _isCheckingInstall;
        private bool _isInitialized;
        private bool _isInitializing;
        private bool _isInitializingStorage;

        public ServiceHandler()
        {
            _isInstalled = false;
            _isInitialized = false;
            _isInitializing = false;

            new OpenDMS.IO.Logger(Properties.Settings.Default.LogDirectory);
            new OpenDMS.Networking.Logger(Properties.Settings.Default.LogDirectory);
            new OpenDMS.Storage.Logger(Properties.Settings.Default.LogDirectory);
            new Logger(Properties.Settings.Default.LogDirectory);

            Logger.Storage.Debug("Instantiating server...");

            _server = new Storage.Providers.CouchDB.Server("http", 
                Properties.Settings.Default.StorageServerAddress,
                Properties.Settings.Default.StorageServerPort,
                Properties.Settings.Default.NetworkTimeout,
                Properties.Settings.Default.NetworkBufferSize);

            Logger.Storage.Debug("Server instantiation complete.");
            Logger.Storage.Debug("Instantiating database...");

            _db = new Storage.Providers.CouchDB.Database(_server,
                Properties.Settings.Default.StorageDatabaseName);

            Logger.Storage.Debug("Database instantiation complete.");
            Logger.Storage.Debug("Instantiating engine...");

            _engine = new Storage.Providers.CouchDB.Engine();

            Logger.Storage.Debug("Engine instantiation complete.");
        }

        public void Init()
        {
            DateTime start = DateTime.Now; // we must set the value here and then reset it below directly before calling, else Visual Studio throws a fit for using an unassigned variable
            Providers.EngineRequest request;

            if (_isInitialized)
                throw new InvalidOperationException("Already initialized.");
            else if (_isInitializing)
                throw new InvalidOperationException("Already initializing.");

            _isInitializing = true;

            request = new Providers.EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.RequestingPartyType = Storage.Security.RequestingPartyType.System;

            request.OnComplete = delegate(Providers.EngineRequest request2, Providers.ICommandReply reply, object result)
            {
                TimeSpan duration = DateTime.Now - start;

                if (!(bool)result)
                {
                    Logger.Storage.Error("OpenDMS.Net is not installed on the database.  Please run installation.  Determined in " + duration.TotalMilliseconds.ToString() + "ms.");
                    _isInstalled = false;
                    return;
                }
                else
                {
                    Logger.Storage.Debug("OpenDMS.Net has detected an installation at " + _db.Uri + " and is now ready to use this installation.  Determined in " + duration.TotalMilliseconds.ToString() + "ms.");
                    _isInstalled = true;
                    InitializeStorage();
                }
            };

            request.OnError += delegate(Providers.EngineRequest request2, string message, Exception exception)
            {
                TimeSpan duration = DateTime.Now - start;

                _isInstalled = false;
                _isInitialized = false;
                _isInitializing = false;

                Logger.Storage.Error("An error occurred while trying to determine if OpenDMS.Net has been installed on the database.  This is not conclusive as to if OpenDMS.Net is properly installed.  " +
                    "We recommend restarting OpenDMS.Net.  Determined in " + duration.TotalMilliseconds.ToString() + "ms.  Error message: " + message, exception);
            };

            // There is no 'progress' to update.
            //request.OnProgress += new Providers.EngineBase.ProgressDelegate(IsInstalled_OnProgress);

            request.OnTimeout += delegate(Providers.EngineRequest request2)
            {
                TimeSpan duration = DateTime.Now - start;

                _isInstalled = false;
                _isInitialized = false;
                _isInitializing = false;

                Logger.Storage.Error("A timeout occurred while trying to determine if OpenDMS.Net has been installed on the database.  This is not conclusive as to if OpenDMS.Net is properly installed.  " +
                    "We recommend restarting OpenDMS.Net.  Determined in " + duration.TotalMilliseconds.ToString() + "ms.");
            };

            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            start = DateTime.Now;
            _engine.DetermineIfInstalled(request, @"C:\Users\Lucas\Documents\Visual Studio 2010\Projects\Test\bin\Debug\");
        }

        private void InitializeStorage()
        {
            if (_isInitializingStorage)
                throw new InvalidOperationException("Initialization of storage already processing.");
            else
                _isInitializingStorage = true;

            List<Storage.Providers.IDatabase> databases = new List<Providers.IDatabase>();
            databases.Add(_db);
            
            Logger.Storage.Debug("Storage engine initialization starting...");
            _engine.Initialize(Properties.Settings.Default.StorageTransactionRootDirectory,
                Properties.Settings.Default.LogDirectory, databases,
                new Providers.EngineBase.InitializationDelegate(Initialize_OnInitialized));
        }

        private void Initialize_OnInitialized(bool success, string message, Exception exception)
        {
            if (!success)
            {
                Logger.Storage.Error("Initialization of the storage engine failed with the following message and/or exception:\r\nMessage: " + message, exception);
                _isInitialized = false;
                _isInitializing = false;
                _isInitializingStorage = false;
                return;
            }

            Logger.Storage.Debug("Storage engine initialization complete.");
            _isInitialized = true;
            _isInitializing = false;
            _isInitializingStorage = false;
        }

        [ServicePoint("/_ping", ServicePointAttribute.VerbType.ALL)]
        public void Ping(HttpApplication app)
        {
            System.IO.Stream stream;
            long contentLength = 0;
            Logger.General.Debug("Responding to ping...");

            Requests.Ping ping = Requests.Request<Requests.Ping>.Parse(app.Request);
            Responses.Pong pong = (Responses.Pong)Responses.Pong.BuildFrom(ping);
            stream = pong.MakeStream(out contentLength);
            stream.CopyTo(app.Response.OutputStream);
            
            app.CompleteRequest();
            Logger.General.Debug("Response pong sent.");
        }

        [ServicePoint("/_auth", ServicePointAttribute.VerbType.POST)]
        public void AuthenticateUser(HttpApplication app)
        {
            Providers.EngineRequest engineRequest;
            System.IO.Stream stream;
            long contentLength = 0;
            Requests.Authentication authReq;
            Responses.Authentication authResp = null;
            bool spinThread = true;

            Logger.General.Debug("Starting handling of authentication request...");

            authReq = Requests.Request<Requests.Authentication>.Parse(app.Request);
                        
            engineRequest = new Providers.EngineRequest()
            {
                Engine = _engine,
                Database = _db,
                RequestingPartyType = Storage.Security.RequestingPartyType.User,
                UserToken = app.Response
            };
            
            engineRequest.OnComplete += delegate(Providers.EngineRequest engineRequest2,
                Providers.ICommandReply reply, object result)
            {
                Tuple<Storage.Security.Session, bool> r = (Tuple<Storage.Security.Session, bool>)result;

                try
                {
                    if (r.Item1 == null)
                        authResp = (Responses.Authentication)Responses.Authentication.BuildFrom(authReq, r.Item2);
                    else
                        authResp = (Responses.Authentication)Responses.Authentication.BuildFrom(authReq, r.Item2, r.Item1.AuthToken, r.Item1.Expiry);
                }
                catch (Exception e)
                {
                    Logger.General.Error("Exception occurred while attempting to create the response object.", e);
                }

                spinThread = false;
            };

            _engine.AuthenticateUser(engineRequest, authReq.Username, authReq.HashedPassword);

            while (spinThread)
            {
                Thread.Sleep(50);
            }

            stream = authResp.MakeStream(out contentLength);
            stream.CopyTo(app.Response.OutputStream);

            app.CompleteRequest();
            Logger.General.Debug("Authentication response sent.");
        }

        //[ServicePoint("/_groups", ServicePointAttribute.VerbType.GET)]
        //public void GetAllGroups(HttpApplication app)
        //{
        //    Providers.EngineRequest request;
        //    Requests.GetAllGroups cmd;
        //    Responses.GroupList resp;
            
        //    cmd = new Requests.GetAllGroups(app.Request);

        //    request = new Providers.EngineRequest()
        //    {
        //        AuthToken = cmd.JObject["AuthToken"].Value<Guid>(),
        //        Database = _db, 
        //        Engine = _engine, 
        //        RequestingPartyType = Storage.Security.RequestingPartyType.User, Session
        //    };
        //    _engine.GetAllGroups(
            
        //    resp = (Responses.GroupList)cmd.BuildResponseMessage(
        //}

        public void Dispose()
        {

        }
    }
}
