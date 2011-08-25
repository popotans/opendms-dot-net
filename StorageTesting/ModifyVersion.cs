using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class ModifyVersion : TestBase
    {
        private DateTime _start;

        public ModifyVersion(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmCheckoutVersion win = new FrmCheckoutVersion();
            win.OnGoClick += new FrmCheckoutVersion.GoDelegate(win_OnGoClick);
            win.ShowDialog();
        }

        void win_OnGoClick(OpenDMS.Storage.Data.VersionId versionId)
        {
            OpenDMS.Storage.Providers.EngineRequest request = new OpenDMS.Storage.Providers.EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Checkout_Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.OnAuthorizationDenied += new EngineBase.AuthorizationDelegate(AuthorizationDenied);
            request.AuthToken = _window.Session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            Clear();

            WriteLine("Downloading the resource for modification...");
            _engine.CheckoutVersion(request, versionId);
        }

        void Checkout_Complete(EngineRequest request, ICommandReply reply, object result)
        {
            Tuple<OpenDMS.Storage.Data.Version, Newtonsoft.Json.Linq.JObject> res =
                (Tuple<OpenDMS.Storage.Data.Version, Newtonsoft.Json.Linq.JObject>)result;

            request = new OpenDMS.Storage.Providers.EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.OnAuthorizationDenied += new EngineBase.AuthorizationDelegate(AuthorizationDenied);
            request.AuthToken = _window.Session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            _start = DateTime.Now;
            WriteLine("Modifying the version...");
            res.Item1.Metadata.Add("$modified", DateTime.Now);
            _engine.ModifyVersion(request, res.Item1);
        }

        private void AuthorizationDenied(EngineRequest request)
        {
            WriteLine("ModifyVersion.AuthorizationDenied - Access to the resource was denied based on usage permissions.");
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("ModifyVersion.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("ModifyVersion.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("ModifyVersion.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;
            Tuple<OpenDMS.Storage.Data.Resource, OpenDMS.Storage.Data.Version> res =
                (Tuple<OpenDMS.Storage.Data.Resource, OpenDMS.Storage.Data.Version>)result;

            WriteLine("ModifyVersion.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            WriteLine("\tId: " + res.Item2.VersionId.ToString());
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("ModifyVersion.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("ModifyVersion.Error - Error.  Message: " + message);
        }
    }
}