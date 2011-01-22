using System;

namespace Common.Data
{
    public class RemoteResource : ResourceBase
    {
        public RemoteResource(Guid guid, FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger) 
            : base(guid, null, fileSystem, generalLogger, networkLogger)
        {
        }

        public override ETag GetETag(Work.ResourceJobBase job)
        {
            return _etag = MetaAsset.GetRemoteETag(job, _generalLogger, _networkLogger);
        }

        public override MetaAsset Load(Work.ResourceJobBase job)
        {
            _metaAsset.DownloadAssetFromServer(job, _fileSystem, _generalLogger, _networkLogger);
            job.UpdateLastAction();
            _dataAsset.DownloadAssetFromServer(job, _fileSystem, _generalLogger, _networkLogger);
            return _metaAsset;
        }
    }
}
