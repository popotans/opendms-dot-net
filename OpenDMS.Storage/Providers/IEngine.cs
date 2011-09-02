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
        bool IsInitializing { get; }

        // Non-sessioned requests
        void AuthenticateUser(EngineRequest request, string username, string hashedPassword);
        void Initialize(string transactionRootDirectory, string logDirectory,
            List<Providers.IDatabase> databases, EngineBase.InitializationDelegate onInitialized);
        void SetState(bool isInitializing, bool isInitialized);

        // Sessioned requests
        void GetAllGroups(EngineRequest request);
        void GetGroup(EngineRequest request, string groupName);
        void CreateGroup(EngineRequest request, Group group);
        void ModifyGroup(EngineRequest request, Group group);

        void GetAllUsers(EngineRequest request);
        void GetUser(EngineRequest request, string username);
        void CreateUser(EngineRequest request, User user);
        void ModifyUser(EngineRequest request, User user);

        void CheckoutResource(EngineRequest request, ResourceId resource);
        void GetResourceReadOnly(EngineRequest request, ResourceId resource);
        void CreateNewResource(EngineRequest request,  CreateResourceArgs args);
        void ModifyResource(EngineRequest request, Resource resource);
        void RollbackResource(EngineRequest request, ResourceId resource, int rollbackDepth);
        void DeleteResource(EngineRequest request, ResourceId resource);

        void CheckoutVersion(EngineRequest request, VersionId version);
        void CheckoutCurrentVersion(EngineRequest request, ResourceId resource);
        void CreateNewVersion(EngineRequest request, CreateVersionArgs args);
        void ModifyVersion(EngineRequest request, Data.Version version);

        // Accomplished by calling GetResource and checking its permissions
        //void GetResourcePermissions(EngineRequest request, ResourceId resource);
        void GetGlobalPermissions(EngineRequest request);
        // Accomplished by calling ModifyResource
        //void ModifyResourcePermissions(EngineRequest request, ResourceId resource, List<UsageRight> usageRights);
        void ModifyGlobalPermissions(EngineRequest request, List<UsageRight> usageRights);
        void GetResourceUsageRightsTemplate(EngineRequest request);
        void ModifyResourceUsageRightsTemplate(EngineRequest request, List<UsageRight> usageRights);

        void Install(EngineRequest request, string logDirectory);
        void DetermineIfInstalled(EngineRequest request, string logDirectory);
    }
}
