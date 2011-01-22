using System;

namespace Common.Data
{
    public abstract class Asset
    {
        private Guid _guid;

        protected AssetType _assetType;
        protected ResourceBase _resource;

        public Guid Guid { get { return _guid; } }
        public ResourceBase Resource { get {  return _resource; } }
        public string GuidString { get { return Guid.ToString("N"); } }
        public abstract string LocalPath { get; }
        public string VirtualPath { get { return _assetType.VirtualPath + "/" + GuidString; } }
        public AssetType AssetType { get { return _assetType; } }

        public Asset()
        {
            _assetType = null;
            _resource = null;
        }

        public Asset(ResourceBase resource, AssetType assetType)
        {
            _resource = resource;
            _guid = _resource.Guid;
            _assetType = assetType;
        }

        public ETag GetRemoteETag(Work.ResourceJobBase job, Logger generalLogger, Logger networkLogger)
        {
            if (Data.AssetType.IsNullOrUnknown(_assetType))
                throw new InvalidOperationException();

            ETag etag;
            Network.Message msg;

            try
            {
                msg = new Network.Message(ServerSettings.Instance.ServerIp, ServerSettings.Instance.ServerPort, 
                    _assetType.VirtualPath, Guid, _assetType, Network.OperationType.HEAD, 
                    Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true, 
                    ServerSettings.Instance.NetworkBufferSize, ServerSettings.Instance.NetworkTimeout,
                    generalLogger, networkLogger);
            }
            catch (Exception e)
            {
                if (generalLogger != null)
                    generalLogger.Write(Logger.LevelEnum.Normal, Logger.ExceptionToString(e));
                throw e;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                if (generalLogger != null)
                    generalLogger.Write(Logger.LevelEnum.Normal, Logger.ExceptionToString(e));
                throw e;
            }
            
            if (job.AbortAction)
            {
                msg.State.Dispose();
                return null;
            }

            if (msg.State.Response.Headers["ETag"] == null)
                return null;

            etag = new ETag(msg.State.Response.Headers["ETag"].Replace("\"", ""));

            if (generalLogger != null)
            {
                generalLogger.Write(Logger.LevelEnum.Debug, "ETag received on " + 
                    job.Id.ToString() + " for " + job.Resource.Guid.ToString() + 
                    " with value of " + etag.Value);
            }

            msg.State.Dispose();

            return etag;
        }

        public void SetGuid(Guid guid)
        {
            if (_resource != null) _resource.SetGuid(guid);
            _guid = guid;
        }
    }
}
