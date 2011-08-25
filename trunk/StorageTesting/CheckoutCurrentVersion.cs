using System;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class CheckoutCurrentVersion : TestBase
    {
        private DateTime _start;

        public CheckoutCurrentVersion(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmGetResource win = new FrmGetResource();
            win.OnGoClick += new FrmGetResource.GoDelegate(win_OnGoClick);
            win.ShowDialog();
        }

        void win_OnGoClick(string id)
        {
            OpenDMS.Storage.Providers.EngineRequest request = new OpenDMS.Storage.Providers.EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.AuthToken = _window.Session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            Clear();

            WriteLine("Starting CheckoutCurrentVersion test...");
            _start = DateTime.Now;
            _engine.CheckoutCurrentVersion(request, new OpenDMS.Storage.Data.ResourceId(id));
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("CheckoutCurrentVersion.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("CheckoutCurrentVersion.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("CheckoutCurrentVersion.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;
            Tuple<OpenDMS.Storage.Data.Resource, Newtonsoft.Json.Linq.JObject,
                OpenDMS.Storage.Data.Version, Newtonsoft.Json.Linq.JObject> res =
                (Tuple<OpenDMS.Storage.Data.Resource, Newtonsoft.Json.Linq.JObject,
                OpenDMS.Storage.Data.Version, Newtonsoft.Json.Linq.JObject>)result;

            OpenDMS.Storage.Providers.CouchDB.Commands.GetDocumentReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetDocumentReply)reply;

            WriteLine("CheckoutCurrentVersion.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            WriteLine("\tResource Id: " + res.Item1.ResourceId.ToString() + " Version No: " + res.Item3.VersionId.VersionNumber.ToString());
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("CheckoutCurrentVersion.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("CheckoutCurrentVersion.Error - Error.  Message: " + message);
        }
    }
}