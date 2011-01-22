using System;

namespace Common.Data
{
    public class LocalResource : ResourceBase
    {
        /// <summary>
        /// Constructor - used when the resource exists locally, but not remotely (e.g., creation of new resource)
        /// </summary>
        /// <param name="filename"></param>
        public LocalResource(FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(fileSystem, generalLogger, networkLogger)
        {
            _etag = new ETag("0");
        }

        public LocalResource(Guid guid, FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(guid, null, fileSystem, generalLogger, networkLogger)
        {
        }

        public LocalResource(Guid guid, ETag etag, FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(guid, etag, fileSystem, generalLogger, networkLogger)
        {
        }

        public override ETag GetETag(Work.ResourceJobBase job)
        {
            if (_metaAsset == null) throw new Exception("The MetaAsset reference cannot be null.");

            if (_metaAsset.ETag != null)
                return _metaAsset.ETag;

            MetaAsset ma = _metaAsset.LoadFromLocal(job, _fileSystem, _generalLogger);
            if (ma != null) return ma.ETag;
            return null;
        }

        public override MetaAsset Load(Work.ResourceJobBase job)
        {
            if (_metaAsset == null) throw new Exception("The MetaAsset reference cannot be null.");

            _metaAsset = _metaAsset.LoadFromLocal(job, _fileSystem, _generalLogger);

            if (_metaAsset != null)
                _etag = _metaAsset.ETag;
            else
                _etag = null;

            return _metaAsset;
        }

        public static LocalResource Load(Guid guid, FileSystem.IO fileSystem, Logger generalLogger,
            Logger networkLogger)
        {
            LocalResource resource = new LocalResource(guid, null, fileSystem, generalLogger, networkLogger);
            resource.MetaAsset.LoadFromLocal(fileSystem, generalLogger);
            resource.SetETag(resource.MetaAsset.ETag);
            return resource;
        }

        public NetworkPackage.ServerResponse[] SaveToServer(Work.SaveResourceJob job, 
            FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
        {
            NetworkPackage.ServerResponse[] sr = new NetworkPackage.ServerResponse[2];

            sr[0] = _metaAsset.SaveAssetToServer((Work.ResourceJobBase)job, 
                fileSystem, generalLogger, networkLogger);

            if (sr[0] == null || !sr[0].ContainsKey("Pass") || !(bool)sr[0]["Pass"])
                return sr;

            job.UpdateLastAction();

            sr[1] = _dataAsset.SaveAssetToServer((Work.ResourceJobBase)job,
                fileSystem, generalLogger, networkLogger);

            if (sr[1] == null || !sr[1].ContainsKey("Pass") || !(bool)sr[1]["Pass"])
                return sr;

            return sr;
        }

        public RemoteResource ToRemoteResource()
        {
            return new RemoteResource(_guid, _fileSystem, _generalLogger, _networkLogger);
        }

        public static explicit operator RemoteResource(LocalResource localResource)
        {
            return localResource.ToRemoteResource();
        }
    }
}
