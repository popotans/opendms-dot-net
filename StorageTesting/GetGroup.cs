using System;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class GetGroup : TestBase
    {
        private DateTime _start;

        public GetGroup(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            FrmGetGroup win = new FrmGetGroup();
            win.OnGoClick += new FrmGetGroup.GoDelegate(win_OnGoClick);
            win.ShowDialog();
        }

        void win_OnGoClick(string groupName)
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

            WriteLine("Starting GetGroup test...");
            _start = DateTime.Now;
            _engine.GetGroup(request, groupName);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("GetGroup.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("GetGroup.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("GetGroup.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            OpenDMS.Storage.Providers.CouchDB.Commands.GetDocumentReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetDocumentReply)reply;

            OpenDMS.Storage.Providers.CouchDB.Transitions.Group txGroup = new OpenDMS.Storage.Providers.CouchDB.Transitions.Group();
            OpenDMS.Storage.Security.Group g = txGroup.Transition(r.Document);
            
            WriteLine("GetGroup.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            WriteLine("\tId: " + g.Id + ", Name: " + g.GroupName);
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("GetGroup.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("GetGroup.Error - Error.  Message: " + message);
        }
    }
}
