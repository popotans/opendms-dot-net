using System;
using System.Collections.Generic;
using OpenDMS.IO;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;
using OpenDMS.Storage.Security;

namespace OpenDMS.Storage.Providers
{
    public interface IEngine
    {
        void AuthenticateUser(IDatabase db, string username, string hashedPassword, EngineBase.AuthenticationDelegate onAuthenticated);
        void Initialize(List<Providers.IDatabase> databases, EngineBase.InitializationDelegate onInitialized);

        void GetAllGroups(EngineRequest request, IDatabase db);
        void GetGroup(EngineRequest request, IDatabase db, string groupName);
        void CreateGroup(EngineRequest request, IDatabase db, Group group);
        void UpdateGroup(EngineRequest request, IDatabase db, Group group);

        void GetAllUsers(EngineRequest request, IDatabase db);
        void GetUser(EngineRequest request, IDatabase db, string username);
        void CreateUser(EngineRequest request, IDatabase db, User user);
        void UpdateUser(EngineRequest request, IDatabase db, User user);

        void CreateNewResource(EngineRequest request, IDatabase db, Metadata metadata, List<UsageRight> usageRights);

        void CreateNewVersion(EngineRequest request, IDatabase db, OpenDMS.Storage.Data.Version version);

    }
}
