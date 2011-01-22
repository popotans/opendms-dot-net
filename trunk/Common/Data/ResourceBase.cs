using System;

namespace Common.Data
{
    public abstract class ResourceBase
    {
        protected Guid _guid;
        protected ETag _etag;
        protected MetaAsset _metaAsset;
        protected DataAsset _dataAsset;
        protected FileSystem.IO _fileSystem;
        protected Logger _generalLogger;
        protected Logger _networkLogger;

        public Guid Guid { get { return _guid; } }
        public ETag ETag { get { return _etag; } }
        public MetaAsset MetaAsset { get { return _metaAsset; } }
        public DataAsset DataAsset { get { return _dataAsset; } }

        public abstract ETag GetETag(Work.ResourceJobBase job);
        public abstract MetaAsset Load(Work.ResourceJobBase job);

        public ResourceBase(FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
        {
            _guid = Guid.Empty;
            _etag = null;
            _metaAsset = new MetaAsset(this);
            _dataAsset = new DataAsset(this);
            _fileSystem = fileSystem;
            _generalLogger = generalLogger;
            _networkLogger = networkLogger;
        }

        public ResourceBase(Guid guid, ETag etag, FileSystem.IO fileSystem, Logger generalLogger,
            Logger networkLogger)
        {
            if (guid == Guid.Empty)
                throw new ArgumentNullException("The guid argument cannot be empty.");
            _guid = guid;
            _metaAsset = new MetaAsset(this);
            _dataAsset = new DataAsset(this);
            _etag = etag;
            _fileSystem = fileSystem;
            _generalLogger = generalLogger;
            _networkLogger = networkLogger;
        }

        public void SetGuid(Guid guid)
        {
            _guid = guid;
        }

        public void SetETag(ETag etag)
        {
            _etag = etag;
        }

        public void SetMetaAsset(MetaAsset ma)
        {
            _metaAsset = ma;
        }
    }
}
