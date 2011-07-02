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
        void GetAllGroups(EngineRequest request);
        void GetGroup(EngineRequest request, string groupName);
        void CreateGroup(EngineRequest request, Group group);
        void ModifyGroup(EngineRequest request, Group group);

        void GetAllUsers(EngineRequest request);
        void GetUser(EngineRequest request, string username);
        void CreateUser(EngineRequest request, User user);
        void ModifyUser(EngineRequest request, User user);

        void GetResource(EngineRequest request, ResourceId resource);
        void GetResourceReadOnly(EngineRequest request, ResourceId resource);
        void CreateNewResource(EngineRequest request, Metadata metadata);
        void CreateNewResource(EngineRequest request, Metadata metadata, List<UsageRight> usageRights);
        void ModifyResource(EngineRequest request, Resource resource);
        void RollbackResource(EngineRequest request, ResourceId resource, int rollbackDepth);
        void DeleteResource(EngineRequest request, ResourceId resource);
        
        void GetVersion(EngineRequest request, VersionId version);
        void GetCurrentVersion(EngineRequest request, ResourceId resource);
        void CreateNewVersion(EngineRequest request, Data.Version version);
        void ModifyVersion(EngineRequest request, Data.Version version);

        void GetResourcePermissions(EngineRequest request, ResourceId resource);
        void GetGlobalPermissions(EngineRequest request);
        // UpdateResourcePermissions - Accomplished by UpdateResource
        void ModifyGlobalPermissions(EngineRequest request, List<UsageRight> usageRights);
        void GetResourceUsageRightsTemplate(EngineRequest request);
        void ModifyResourceUsageRightsTemplate(EngineRequest request, List<UsageRight> usageRights);
    }
}
