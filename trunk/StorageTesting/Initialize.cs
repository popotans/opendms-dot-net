using System;
using System.Collections.Generic;
using OpenDMS.Storage.Providers;

namespace StorageTesting
{
    public class Initialize : TestBase
    {
        public delegate void InitializationDelegate(bool success);
        public event InitializationDelegate OnInitialized;

        private DateTime _start;

        public Initialize(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            EngineRequest request = new EngineRequest();

            request.Engine = _engine;
            request.Database = _db;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            Clear();

            WriteLine("Starting initialization test...");
            _start = DateTime.Now;

            List<OpenDMS.Storage.Providers.IDatabase> databases = new List<OpenDMS.Storage.Providers.IDatabase>();
            databases.Add(_db);
            _engine.Initialize(@"C:\Users\Lucas\Documents\Visual Studio 2010\Projects\Test\bin\Debug\Transactions\", 
                @"C:\Users\Lucas\Documents\Visual Studio 2010\Projects\Test\bin\Debug\",
                databases, Engine_OnInitialized);
        }

        private void Engine_OnInitialized(bool success, string message, Exception exception)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            if (success)
                WriteLine("Initialize.Complete - Engine initialization was successful in " + duration.TotalMilliseconds.ToString() + "ms.");
            else
                WriteLine("Initialize.Complete - Engine initialization failed in " + duration.TotalMilliseconds.ToString() + "ms, with message: " + message);

            OnInitialized(success);
        }
    }
}
