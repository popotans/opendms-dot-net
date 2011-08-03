using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class Install : Base
    {
        

        public Install(EngineRequest request)
            : base(request)
        {
        }

        public override void Execute()
        {
            //Model.BulkDocuments package;
            //Commands.PostBulkDocuments cmd;

            //if (_request.RequestingPartyType != Security.RequestingPartyType.System)
            //{
            //    // Not authorized
            //    if (_onAuthorizationDenied != null) _onAuthorizationDenied(_request);
            //    else throw new NotImplementedException("OnAuthorizationDenied must have a subscriber.");
            //    return;
            //}
                
            //try
            //{
            //    if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, false);
            //}
            //catch (System.Exception e)
            //{
            //    Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
            //    throw;
            //}

            ////Newtonsoft.Json.Linq.JObject jobj = Newtonsoft.Json.Linq.JObject.Parse(_package);
            //package = new Model.BulkDocuments(_package);
            //package.AllOrNothing = true;
            //cmd = new Commands.PostBulkDocuments(_request.Database, package);

            //AttachSubscriberEvent(cmd, _request.OnProgress);
            //AttachSubscriberEvent(cmd, _request.OnComplete);
            //AttachSubscriberEvent(cmd, _request.OnError);
            //AttachSubscriberEvent(cmd, _request.OnTimeout);
            
            //try
            //{
            //    if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Installing, true);
            //}
            //catch (System.Exception e)
            //{
            //    Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
            //    throw;
            //}

            //cmd.Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
        }

    }
}
