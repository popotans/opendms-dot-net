using System;
using System.ComponentModel;
using Common.FileSystem;

namespace Common.Data
{
    public class DataAsset 
        : AssetBase
    {
        public delegate void EventHandler(DataAsset sender);
        public delegate void ProgressHandler(DataAsset sender, int percentComplete);
        public event ProgressHandler OnProgress;
        public event EventHandler OnComplete;

        public ulong BytesComplete;
        public ulong BytesTotal;

        public FileSystem.DataResource Resource
        {
            get { return (FileSystem.DataResource)_resource; }
        }

        public DataAsset(Logger logger)
            : base(logger)
        {
            BytesComplete = BytesTotal = 0;
        }

        public DataAsset(MetaAsset ma, FileSystem.IO fileSystem, Logger logger)
            : this(ma.Guid, ma.Extension, fileSystem, logger)
        {
        }

        public DataAsset(Guid guid, string extension, FileSystem.IO fileSystem, Logger logger)
            : base(guid, AssetType.Data, logger)
        {
            _resource = new FileSystem.DataResource(guid, extension, fileSystem, logger);
            BytesComplete = BytesTotal = 0;
            _state = new AssetState() { State = AssetState.Flags.CanTransfer };
        }

        public bool CopyCurrentToVersionScheme(UInt64 version)
        {
            return _resource.CopyCurrentToVersionScheme(version,
                "Common.Data.MetaAsset.CopyCurrentToVersionScheme()");
        }

        public bool DownloadFromServer(Work.AssetJobBase job, MetaAsset ma, Logger networkLogger)
        {
            if (!_state.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(_state, "Cannot download");

            IOStream iostream;
            Network.Message msg;
            byte[] buffer = new byte[ServerSettings.Instance.NetworkBufferSize];
            int bytesRead = 0;
            int percentDone = 0;

            try
            {
                // Resolved : why is this using the Memory DataStreamMethod?
                // Reason: this specifies how the request is sent to the server, nothing to do with response
                msg = new Network.Message(ServerSettings.Instance.ServerIp, ServerSettings.Instance.ServerPort,
                    _assetType.VirtualPath, Guid, _assetType, Network.OperationType.GET,
                    Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true,
                    ServerSettings.Instance.NetworkBufferSize, ServerSettings.Instance.NetworkTimeout,
                    _logger, networkLogger);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, Logger.ExceptionToString(e));
                throw e;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, Logger.ExceptionToString(e));
                throw e;
            }

            if (job.AbortAction)
            {
                msg.State.Dispose();
                return false;
            }

            _resource.CreateContainingDirectory();

            BytesTotal = ma.Length;
            BytesComplete = 0;

            try
            {
                iostream = _resource.GetExclusiveWriteStream("Common.Data.DataAsset.DownloadAssetFromServer()");
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            if (iostream == null)
            {
                job.SetErrorFlag();
                return false;
            }

            while ((bytesRead = msg.State.Stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (job.AbortAction)
                {
                    _resource.CloseStream();
                    msg.State.Dispose();
                    return false;
                }

                iostream.Write(buffer, bytesRead);
                BytesComplete = BytesComplete + (uint)bytesRead;
                job.UpdateLastAction();
                if (OnProgress != null)
                {
                    percentDone = (int)(((double)BytesComplete / (double)BytesTotal) * 100D);
                    OnProgress(this, percentDone);
                }
            }

            // Close the writer
            _resource.CloseStream();

            // Dispose network assets
            msg.State.Dispose();

            if (job.AbortAction)
            {
                msg.State.Dispose();
                return false;
            }

            if (OnComplete != null) OnComplete(this);

            return true;
        }

        public NetworkPackage.ServerResponse SaveToServer(Work.AssetJobBase job, MetaAsset ma,
            Logger networkLogger)
        {
            if (!_state.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(_state, "Cannot download");

            NetworkPackage.ServerResponse sr;
            Network.Message msg;
            IOStream iostream;

            try
            {
                iostream = _resource.GetExclusiveReadStream("Common.Data.DataAsset.SaveAssetToServer()");
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            // If we have a null iostream -> get out of this
            if (iostream == null)
            {
                job.SetErrorFlag();
                return null;
            }

            try
            {
                msg = new Network.Message(ServerSettings.Instance.ServerIp, ServerSettings.Instance.ServerPort,
                    _assetType.VirtualPath, _guid, _assetType, Network.OperationType.PUT,
                    Network.DataStreamMethod.Stream, iostream.Stream, null, iostream.Stream.Length,
                    null, false, false, false, false, ServerSettings.Instance.NetworkBufferSize,
                    ServerSettings.Instance.NetworkTimeout, _logger, networkLogger);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, Logger.ExceptionToString(e));
                throw e;
            }

            if (job.AbortAction)
            {
                _resource.CloseStream();
                msg.State.Dispose();
                return null;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to send the asset to the server.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            _resource.CloseStream();

            sr = new NetworkPackage.ServerResponse();

            try
            {
                sr.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to deserialize the result.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            return sr;
        }

        public IOStream GetReadStream()
        {
            IOStream iostream;

            try
            {
                iostream = _resource.GetExclusiveReadStream("Common.Data.DataAsset.GetReadStream()");
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            return iostream;
        }

        public IOStream GetWriteStream()
        {
            IOStream iostream;

            try
            {
                iostream = _resource.GetExclusiveWriteStream("Common.Data.DataAsset.GetWriteStream()");
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            return iostream;
        }
    }
}
