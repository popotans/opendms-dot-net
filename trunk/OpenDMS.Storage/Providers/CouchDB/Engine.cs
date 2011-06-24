using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine : EngineBase
    {
        public Engine()
            : base()
        {
        }

        public override void GetAllGroups(EngineRequest request, IDatabase db)
        {
            EngineMethods.GetAllGroups act = new EngineMethods.GetAllGroups(db, request.OnActionChanged, request.OnProgress, request.OnComplete, request.OnTimeout, request.OnError);
            act.Execute();
        }

        public override void GetGroup(EngineRequest request, IDatabase db, string groupName)
        {
            EngineMethods.GetGroup act = new EngineMethods.GetGroup(db, groupName, request.OnActionChanged, request.OnProgress, request.OnComplete, request.OnTimeout, request.OnError);
            act.Execute();
        }

        public virtual void CreateNewResource(EngineRequest request, IDatabase db, Data.Metadata metadata, System.Collections.Generic.List<Security.UsageRight> usageRights)
        {
            EngineMethods.CreateNewResource act = new EngineMethods.CreateNewResource(db, metadata, usageRights, request.OnActionChanged, request.OnProgress, request.OnComplete, request.OnTimeout, request.OnError);
            act.Execute();
        }

        public virtual void CreateNewVersion(EngineRequest request, IDatabase db, Data.Version version)
        {
            EngineMethods.CreateNewVersion act = new EngineMethods.CreateNewVersion(db, version, request.OnActionChanged, request.OnProgress, request.OnComplete, request.OnTimeout, request.OnError);
            act.Execute();
        }
    }
}
