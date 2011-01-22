using System;

namespace Common.FileSystem
{
    public class MetaResource 
        : ResourceBase
    {
        public MetaResource(Guid guid, IO fileSystem, Logger logger)
            : base(guid, ResourceType.Meta, ".xml", fileSystem, logger)
        {
        }
    }
}
