using System;
using OpenDMS.IO;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers
{
    public interface IEngine
    {
        void GetAllGroups(EngineRequest request, IDatabase db);
    }
}
