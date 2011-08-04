using System;
using OpenDMS.Storage.Providers;

namespace StorageTesting
{
    public class Install : TestBase
    {
        public delegate void InstallDelegate();
        public event InstallDelegate OnInstallSuccess;

        private DateTime _start;

        public Install(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            EngineRequest request = new EngineRequest();

            request.Engine = _engine;
            request.Database = _db;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;
            request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);

            Clear();

            WriteLine("Starting installation test...");
            _start = DateTime.Now;
            _engine.Install(request, @"C:\Users\Lucas\Documents\Visual Studio 2010\Projects\Test\bin\Debug\");
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("Install.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("Install.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("Install.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;
            int successQty = 0;

            OpenDMS.Storage.Providers.CouchDB.Commands.PostBulkDocumentsReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.PostBulkDocumentsReply)reply;
            
            WriteLine("Install.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            for (int i = 0; i < r.Results.Count; i++)
            {
                if (r.Results[i].Error != null)
                    WriteLine("\t" + r.Results[i].Id + " : " + r.Results[i].Error);
                else
                {
                    successQty++;
                    WriteLine("\t" + r.Results[i].Id + " : success.");
                }
            }

            if (successQty == r.Results.Count)
                OnInstallSuccess();
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("Install.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("Install.Error - Error.  Message: " + message);
        }
    }
}
