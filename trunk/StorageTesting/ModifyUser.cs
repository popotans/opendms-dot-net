using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class ModifyUser : TestBase
    {
        private DateTime _start;

        public ModifyUser(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmModifyUser win = new FrmModifyUser(_engine, _db, _window.Session);
            win.OnSaveClick += new FrmModifyUser.SaveDelegate(win_OnSaveClick);
            win.ShowDialog();
        }

        void win_OnSaveClick(OpenDMS.Storage.Security.User user)
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

            WriteLine("Starting ModifyUser test...");
            _start = DateTime.Now;
            _engine.ModifyUser(request, user);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("ModifyUser.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("ModifyUser.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("ModifyUser.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            OpenDMS.Storage.Providers.CouchDB.Commands.PutDocumentReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.PutDocumentReply)reply;

            if (r.Ok)
                WriteLine("ModifyUser.Complete - success in " + duration.TotalMilliseconds.ToString() + "ms.");
            else
                WriteLine("ModifyUser.Complete - failed in " + duration.TotalMilliseconds.ToString() + "ms.");
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("ModifyUser.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("ModifyUser.Error - Error.  Message: " + message);
        }
    }
}
