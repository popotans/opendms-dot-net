using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class DeleteResource : TestBase
    {
        private DateTime _start;

        public DeleteResource(FrmMain window, IEngine engine, IDatabase db)
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
            request.OnAuthorizationDenied += new EngineBase.AuthorizationDelegate(AuthorizationDenied);
            request.AuthToken = _window.Session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.User;

            Clear();

            WriteLine("Starting DeleteResource test...");
            _start = DateTime.Now;
            _engine.DeleteResource(request, new OpenDMS.Storage.Data.ResourceId(id));
        }

        private void AuthorizationDenied(EngineRequest request)
        {
            WriteLine("DeleteResource.AuthorizationDenied - Access to the resource was denied based on usage permissions.");
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("DeleteResource.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("DeleteResource.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("DeleteResource.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;
            List<OpenDMS.Storage.Providers.CouchDB.Commands.PostBulkDocumentsReply.Entry> results = (List<OpenDMS.Storage.Providers.CouchDB.Commands.PostBulkDocumentsReply.Entry>)result;

            WriteLine("DeleteResource.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            for(int i=0; i<results.Count; i++)
                WriteLine("\tId: " + results[i].Id.ToString());
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("DeleteResource.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("DeleteResource.Error - Error.  Message: " + message);
        }
    }
}
