using System;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine
    {
        public Engine()
        {
        }

        #region Resource Actions

        public void CreateNewResource(Metadata metadata, Content content)
        {
            // Get a unique resource id
            // Build version
            // Translate version
            // PutDocument
            // PutAttachment
        }

        public void GetCurrentVersion(Data.ResourceId id)
        {
        }

        public void GetResource(Data.ResourceId id)
        {
        }

        public void DeleteResource(Data.ResourceId id)
        {
        }

        public void Revert(Data.VersionId id)
        {
        }

        #endregion

        #region Version Actions

        public void CreateNewVersion(Data.Version version)
        {
        }

        public void GetCurrentVersion(Data.VersionId id)
        {
            GetCurrentVersion(id.ResourceId);
        }

        #endregion
    }
}
