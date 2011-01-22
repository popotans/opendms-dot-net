using System;

namespace Common.Data
{
    public abstract class AssetBase
    {
        protected Guid _guid;
        protected AssetType _assetType;
        protected FileSystem.ResourceBase _resource;
        protected Logger _logger;
        protected AssetState _state;

        public Guid Guid { get { return _guid; } }
        public string GuidString { get { return Guid.ToString("N"); } }
        public AssetType AssetType { get { return _assetType; } }
        public AssetState AssetState { get { return _state; } }

        public AssetBase(Logger logger) : 
            this(Guid.Empty, null, logger)
        {
        }

        public AssetBase(AssetType assetType, Logger logger)
            : this(Guid.Empty, assetType, logger)
        {
        }

        public AssetBase(Guid guid, AssetType assetType, Logger logger)
        {
            _guid = guid;
            _assetType = assetType;
            _resource = null;
            _logger = logger;
            _state = new AssetState(AssetState.Flags.None);
        }

        public bool ResourceExistsOnFilesystem()
        {
            return _resource.ExistsOnFilesystem();
        }

        //public void SetGuid(Guid guid)
        //{
        //    if (_resource != null) _resource.SetGuid(guid);
        //    _guid = guid;
        //}
    }
}
