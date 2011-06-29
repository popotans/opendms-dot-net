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
        // Non-sessioned requests
        void AuthenticateUser(IDatabase db, string username, string hashedPassword, EngineBase.AuthenticationDelegate onAuthenticated);
        void Initialize(List<Providers.IDatabase> databases, EngineBase.InitializationDelegate onInitialized);

        // Sessioned requests
        void GetAllGroups(EngineRequest request, IDatabase db);
        void GetGroup(EngineRequest request, IDatabase db, string groupName);
        void CreateGroup(EngineRequest request, IDatabase db, Group group);
        void UpdateGroup(EngineRequest request, IDatabase db, Group group);

        void GetAllUsers(EngineRequest request, IDatabase db);
        void GetUser(EngineRequest request, IDatabase db, string username);
        void CreateUser(EngineRequest request, IDatabase db, User user);
        void UpdateUser(EngineRequest request, IDatabase db, User user);

        void GetResource(EngineRequest request, IDatabase db, ResourceId resource);
        void GetResourceReadOnly(EngineRequest request, IDatabase db, ResourceId resource);
        void CreateNewResource(EngineRequest request, IDatabase db, Metadata metadata);
        void CreateNewResource(EngineRequest request, IDatabase db, Metadata metadata, List<UsageRight> usageRights);
        void ModifyResource(EngineRequest request, IDatabase db, Resource resource);
        void RollbackResource(EngineRequest request, IDatabase db, ResourceId resource, int rollbackDepth);
        void DeleteResource(EngineRequest request, IDatabase db, ResourceId resource);
        
        void GetVersion(EngineRequest request, IDatabase db, VersionId version);
        void GetCurrentVersion(EngineRequest request, IDatabase db, ResourceId resource);
        void CreateNewVersion(EngineRequest request, IDatabase db, OpenDMS.Storage.Data.Version version);
        void UpdateVersion(EngineRequest request, IDatabase db, Data.Version version);
        void ModifyVersion(EngineRequest request, IDatabase db, Data.Version version);

        void GetResourcePermissions(EngineRequest request, IDatabase db, ResourceId resource);
        void GetGlobalPermissions(EngineRequest request, IDatabase db);
        // UpdateResourcePermissions - Accomplished by UpdateResource
        void UpdateGlobalPermissions(EngineRequest request, IDatabase db, List<UsageRight> usageRights);
        void GetResourceUsageRightsTemplate(EngineRequest request, IDatabase db);
        void ModifyResourceUsageRightsTemplate(EngineRequest request, IDatabase db, List<UsageRight> usageRights);
    }
}
