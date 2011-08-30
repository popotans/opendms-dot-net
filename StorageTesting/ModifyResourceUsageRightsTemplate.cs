using System;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Providers;
using OpenDMS.Storage.Providers.CouchDB;

namespace StorageTesting
{
    public class ModifyResourceUsageRightsTemplate : TestBase
    {
        private DateTime _start;

        public ModifyResourceUsageRightsTemplate(FrmMain window, IEngine engine, IDatabase db)
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

            WriteLine("Starting ModifyResourceUsageRightsTemplate test...");

            OpenDMS.Storage.Security.UsageRight ur1 = new OpenDMS.Storage.Security.UsageRight(new OpenDMS.Storage.Security.Group("administrators"), OpenDMS.Storage.Security.Authorization.ResourcePermissionType.All);
            OpenDMS.Storage.Security.UsageRight ur2 = new OpenDMS.Storage.Security.UsageRight(new OpenDMS.Storage.Security.Group("users"), 
                OpenDMS.Storage.Security.Authorization.ResourcePermissionType.Checkout | 
                OpenDMS.Storage.Security.Authorization.ResourcePermissionType.ReadOnly);
            System.Collections.Generic.List<OpenDMS.Storage.Security.UsageRight> list = new System.Collections.Generic.List<OpenDMS.Storage.Security.UsageRight>();
            list.Add(ur1);
            list.Add(ur2);

            _start = DateTime.Now;

            _engine.ModifyResourceUsageRightsTemplate(request, list);
        }

        private void EngineAction(EngineRequest request, EngineActionType actionType, bool willSendProgress)
        {
            if (willSendProgress)
                WriteLine("ModifyResourceUsageRightsTemplate.EngineAction - Type: " + actionType.ToString() + " Expecting Progress Reports.");
            else
                WriteLine("ModifyResourceUsageRightsTemplate.EngineAction - Type: " + actionType.ToString() + " NOT Expecting Progress Reports.");
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("ModifyResourceUsageRightsTemplate.Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        private void Complete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;

            ResourceUsageRightsTemplate template = (ResourceUsageRightsTemplate)result;

            WriteLine("ModifyResourceUsageRightsTemplate.Complete - results received in " + duration.TotalMilliseconds.ToString() + "ms.");
        }

        private void Timeout(EngineRequest request)
        {
            WriteLine("ModifyResourceUsageRightsTemplate.Timeout - Timeout.");
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
            WriteLine("ModifyResourceUsageRightsTemplate.Error - Error.  Message: " + message);
        }
    }
}
