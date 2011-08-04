using System;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class GetUser : TestBase
    {
        private DateTime _start;

        public GetUser(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmGetUser win = new FrmGetUser();
            win.OnGoClick += new FrmGetUser.GoDelegate(win_OnGoClick);
            win.ShowDialog();
        }

        void win_OnGoClick(string username)
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

            WriteLine("Starting GetUser test...");
            _start = DateTime.Now;
            _engine.GetUser(request, username);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("GetUser.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("GetUser.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("GetUser.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            OpenDMS.Storage.Providers.CouchDB.Commands.GetDocumentReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetDocumentReply)reply;

            OpenDMS.Storage.Providers.CouchDB.Transitions.User txUser = new OpenDMS.Storage.Providers.CouchDB.Transitions.User();
            OpenDMS.Storage.Security.User user = txUser.Transition(r.Document);

            WriteLine("GetUser.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            WriteLine("\tId: " + user.Id + ", Name: " + user.Username);
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("GetUser.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("GetUser.Error - Error.  Message: " + message);
        }
    }
}
