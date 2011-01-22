using System;

namespace Common.FileSystem
{
    public class DataResource
        : ResourceBase
    {
        public DataResource(Guid guid, string extension, IO fileSystem, Logger logger)
            : base(guid, ResourceType.Data, extension, fileSystem, logger)
        {
        }
    }
}
