using System;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class GetResourceUsageRightsTemplate : Base
    {
        public GetResourceUsageRightsTemplate(EngineRequest request)
            : base(request)
        {
        }

        public override void Execute()
        {
            // Anyone can get the resourceusagerightstemplate, thus no permission checking

            Commands.GetDocument cmd;

            try
            {
                cmd = new Commands.GetDocument(UriBuilder.Build(_request.Database, "resourceusagerightstemplate"));
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDocument command.", e);
                throw;
            }

            // Run it straight back to the subscriber
            AttachSubscriberEvent(cmd, _onProgress);
            AttachSubscriberEvent(cmd, _onComplete);
            AttachSubscriberEvent(cmd, _onError);
            AttachSubscriberEvent(cmd, _onTimeout);

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.GettingResourceUsageRightsTemplate, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            try
            {
                cmd.Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while executing the command.", e);
            }
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Not called
            throw new NotImplementedException();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Never called
            throw new System.NotImplementedException();
        }
    }
}
