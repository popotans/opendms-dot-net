using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class ModifyResource : TestBase
    {
        private DateTime _start;

        public ModifyResource(FrmMain window, IEngine engine, IDatabase db)
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
            request.OnComplete += new EngineBase.CompletionDelegate(Checkout_Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.OnAuthorizationDenied += new EngineBase.AuthorizationDelegate(AuthorizationDenied);
            request.AuthToken = _window.Session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            Clear();

            WriteLine("Downloading the resource for modification...");
            _engine.CheckoutResource(request, new OpenDMS.Storage.Data.ResourceId(id));
        }

        void Checkout_Complete(EngineRequest request, ICommandReply reply, object result)
        {
            Tuple<OpenDMS.Storage.Data.Resource, Newtonsoft.Json.Linq.JObject> r = (Tuple<OpenDMS.Storage.Data.Resource, Newtonsoft.Json.Linq.JObject>)result;

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
            WriteLine("Modifying the resource...");
            r.Item1.Metadata.Add("$modified", DateTime.Now);
            _engine.ModifyResource(request, r.Item1);
        }

        private void AuthorizationDenied(EngineRequest request)
        {
            WriteLine("ModifyResource.AuthorizationDenied - Access to the resource was denied based on usage permissions.");
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("ModifyResource.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("ModifyResource.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("ModifyResource.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            OpenDMS.Storage.Providers.CouchDB.Commands.PutDocumentReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.PutDocumentReply)reply;

            if (r.Ok)
                WriteLine("ModifyResource.Complete - success in " + duration.TotalMilliseconds.ToString() + "ms.");
            else
                WriteLine("ModifyResource.Complete - failed in " + duration.TotalMilliseconds.ToString() + "ms.");
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("ModifyResource.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("ModifyResource.Error - Error.  Message: " + message);
        }
    }
}
