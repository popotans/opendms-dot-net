using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class ModifyGroup : TestBase
    {
        private DateTime _start;

        public ModifyGroup(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmModifyGroup win = new FrmModifyGroup(_engine, _db, _window.Session);
            win.OnSaveClick += new FrmModifyGroup.SaveDelegate(win_OnSaveClick);
            win.ShowDialog();
        }

        void win_OnSaveClick(OpenDMS.Storage.Security.Group group)
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
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            Clear();

            WriteLine("Starting ModifyGroup test...");
            _start = DateTime.Now;
            _engine.ModifyGroup(request, group);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("ModifyGroup.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("ModifyGroup.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("ModifyGroup.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            OpenDMS.Storage.Providers.CouchDB.Commands.PutDocumentReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.PutDocumentReply)reply;

            if (r.Ok)
                WriteLine("ModifyGroup.Complete - success in " + duration.TotalMilliseconds.ToString() + "ms.");
            else
                WriteLine("ModifyGroup.Complete - failed in " + duration.TotalMilliseconds.ToString() + "ms.");
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("ModifyGroup.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("ModifyGroup.Error - Error.  Message: " + message);
        }
    }
}
