/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Common.FileSystem;

namespace Common.Data
{
    /// <summary>
    /// A <see cref="DataAsset"/> represents a data file in a way that it is usable by the OpenDMS.NET Project.
    /// </summary>
    public sealed class DataAsset 
        : AssetBase
    {
        /// <summary>
        /// Represents the method that handles an event.
        /// </summary>
        /// <param name="sender">The <see cref="DataAsset"/> that triggered the event.</param>
        public delegate void EventHandler(DataAsset sender);
        /// <summary>
        /// Represents the method that handles a progress event.
        /// </summary>
        /// <param name="sender">The <see cref="DataAsset"/> that triggered the event.</param>
        /// <param name="percentComplete">An integer value representing the percentage of progress.</param>
        public delegate void ProgressHandler(DataAsset sender, int percentComplete);
        /// <summary>
        /// Occurs when progress is made in a long running action.
        /// </summary>
        public event ProgressHandler OnProgress;
        /// <summary>
        /// Occurs when a long running action is completed.
        /// </summary>
        public event EventHandler OnComplete;

        /// <summary>
        /// The quantity of bytes completed.
        /// </summary>
        public ulong BytesComplete;
        /// <summary>
        /// The total quantity of bytes.
        /// </summary>
        public ulong BytesTotal;

        /// <summary>
        /// Gets the reference to the <see cref="FileSystem.DataResource"/> giving this 
        /// <see cref="DataAsset"/> access to the file system.
        /// </summary>
        public FileSystem.DataResource Resource
        {
            get { return (FileSystem.DataResource)_resource; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="logger">A reference to the <see cref="Logger"/> that this instance should use to document events.</param>
        public DataAsset(Logger logger)
            : base(logger)
        {
            BytesComplete = BytesTotal = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="ma">The <see cref="MetaAsset"/> that is paired with this <see cref="DataAsset"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> that this instance should use to document events.</param>
        public DataAsset(MetaAsset ma, FileSystem.IO fileSystem, Logger logger)
            : this(ma.Guid, ma.Extension, fileSystem, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> providing a unique reference to the Asset.</param>
        /// <param name="extension">The extension of the resource (e.g., .doc, .xsl, .odt)</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> that this instance should use to document events.</param>
        public DataAsset(Guid guid, string extension, FileSystem.IO fileSystem, Logger logger)
            : base(guid, AssetType.Data, logger)
        {
            _resource = new FileSystem.DataResource(guid, extension, fileSystem, logger);
            BytesComplete = BytesTotal = 0;
            _state = new AssetState() { State = AssetState.Flags.CanTransfer };
        }

        /// <summary>
        /// Copies the current version of this <see cref="DataAsset"/> to a file using the version scheme for numbering.
        /// </summary>
        /// <param name="version">The version number to use when titling the file</param>
        /// <returns><c>True</c> if successful, <c>false</c> otherwise.</returns>
        /// <example>
        /// This sample shows how to call the <see cref="CopyCurrentToVersionScheme"/> method.
        /// <code>
        /// // This code assumes _dataAsset is instantiated as a <see cref="DataAsset"/> and _metaAsset is instantiated 
        /// // as a <see cref="MetaAsset"/>.
        /// void A()
        /// {
        ///     if (!_dataAsset.CopyCurrentToVersionScheme(_metaAsset.DataVersion))
        ///     {
        ///         MessageBox.Show("Failed to copy");
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool CopyCurrentToVersionScheme(UInt64 version)
        {
            return _resource.CopyCurrentToVersionScheme(version,
                "Common.Data.MetaAsset.CopyCurrentToVersionScheme()");
        }

        /// <summary>
        /// Downloads the current version of this <see cref="DataAsset"/> from the server and writes it 
        /// directly to the file system.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.AssetJobBase"/> which has called this method.</param>
        /// <param name="ma">A reference to the <see cref="MetaAsset"/> that corresponds to this <see cref="DataAsset"/>.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> that is used to document network events.</param>
        /// <returns><c>True</c> if successful, <c>false</c> otherwise.</returns>
        /// <example>
        /// This sample shows how to call the <see cref="DownloadFromServer"/> method.
        /// <code>
        /// // This code assumes _dataAsset is instantiated as a <see cref="DataAsset"/> and _metaAsset
        /// // is instantiated as a <see cref="MetaAsset"/>.
        /// void A()
        /// {
        ///     if (!_dataAsset.DownloadFromServer(job, _metaAsset, networkLogger))
        ///     {
        ///         job.SetErrorFlag();
        ///         return;
        ///     }
        /// }       
        /// </code>
        /// </example>
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

        /// <summary>
        /// Saves the current version of this <see cref="DataAsset"/> to the server.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.AssetJobBase"/> which has called this method.</param>
        /// <param name="ma">A reference to the <see cref="MetaAsset"/> that corresponds to this <see cref="DataAsset"/>.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> that is used to document network events.</param>
        /// <returns>A <see cref="NetworkPackage.ServerResponse"/> representing the result of the save.</returns>
        /// <example>
        /// This sample shows how to call the <see cref="SaveToServer"/> method.
        /// <code>
        /// // This code assumes _dataAsset is instantiated as a <see cref="DataAsset"/>, the job and networkLogger
        /// // are passed as arguments in this example.
        /// void A(Work.AssetJobBase job, Logger networkLogger)
        /// {
        ///     NetworkPackage.ServerResponse sr = _dataAsset.SaveToServer(job, networkLogger);
        ///     
        ///     if (!(bool)sr["Pass"])
        ///     {
        ///         job.SetErrorFlag();
        ///         return;
        ///     }
        /// }
        /// </code>
        /// </example>
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

        /// <summary>
        /// Gets a <see cref="IOStream"/> for use in reading the data from the file system.
        /// </summary>
        /// <returns>A <see cref="IOStream"/> for reading.</returns>
        /// <remarks>Make sure to call FileSystem.IO.Close() once access is done.</remarks>
        /// <example>
        /// This sample shows how to call the <see cref="GetReadStream"/> method.
        /// <code>
        /// // This code assumes _dataAsset is instantiated as a <see cref="DataAsset"/> and _fileSystem is 
        /// // instantiated as <see cref="FileSystem.IO"/>.
        /// void A()
        /// {
        ///     // Open for read
        ///     IOStream iostream = _dataAsset.GetReadStream();
        ///     
        ///     // Close
        ///     _fileSystem.Close(iostream);
        /// }
        /// </code>
        /// </example>
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

        /// <summary>
        /// Gets a <see cref="IOStream"/> for use in writing data to the file system.
        /// </summary>
        /// <returns>A <see cref="IOStream"/> for writing.</returns>
        /// <remarks>Make sure to call FileSystem.IO.Close() once access is done.</remarks>
        /// <example>
        /// This sample shows how to call the <see cref="GetReadStream"/> method.
        /// <code>
        /// // This code assumes _dataAsset is instantiated as a <see cref="DataAsset"/> and _fileSystem is 
        /// // instantiated as <see cref="FileSystem.IO"/>.
        /// void A()
        /// {
        ///     // Open for write
        ///     IOStream iostream = _dataAsset.GetWriteStream();
        ///     
        ///     // Close
        ///     _fileSystem.Close(iostream);
        /// }
        /// </code>
        /// </example>
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
