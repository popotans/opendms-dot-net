using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class GetAllUsers : TestBase
    {
        private DateTime _start;

        public GetAllUsers(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
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

            WriteLine("Starting GetAllUsers test...");
            _start = DateTime.Now;
            _engine.GetAllUsers(request);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("GetAllUsers.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("GetAllUsers.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("GetAllUsers.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply)reply;

            WriteLine("GetAllUsers.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");

            OpenDMS.Storage.Providers.CouchDB.Transitions.UserCollection uc = new OpenDMS.Storage.Providers.CouchDB.Transitions.UserCollection();
            List<OpenDMS.Storage.Security.User> users = uc.Transition(r.View);

            WriteLine("The following groups were loaded:");
            for (int i=0; i<users.Count; i++)
            {
                WriteLine("\t" + users[i].Username);
            }
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("GetAllUsers.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("GetAllUsers.Error - Error.  Message: " + message);
        }
    }
}
