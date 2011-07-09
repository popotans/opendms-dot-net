using System;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class GetAllGroups : TestBase
    {
        public GetAllGroups(FrmMain window, IEngine engine, IDatabase db)
            : base(window, engine, db)
        {
        }

        public override void Test()
        {
            OpenDMS.Storage.Providers.EngineRequest request = new OpenDMS.Storage.Providers.EngineRequest();
            request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            _engine.GetAllGroups(request);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                Console.WriteLine("Storage.GetAllGroups.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                Console.WriteLine("Storage.GetAllGroups.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            Console.WriteLine("Storage.GetAllGroups.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply)reply;
            Console.WriteLine("Storage.GetAllGroups.Complete - Complete success: " + r.Ok.ToString());
        }

        private void Timeout(EngineRequest request)
        {
            Console.WriteLine("Storage.GetAllGroups.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            Console.WriteLine("Storage.GetAllGroups.Error - Error.");
        }
    }
}
