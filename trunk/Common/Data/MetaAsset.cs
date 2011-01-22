using System;
using System.IO;
using System.Collections.Generic;

namespace Common.Data
{
    public sealed class MetaAsset 
        : AssetBase
    {
        #region Meta Data - The following are mandatory properties for metadata

        private ETag _etag;
        private uint _metaversion;
        private uint _dataversion;
        private string _lockedby;
        private DateTime? _lockedat;
		private string _creator;
        private ulong _length;
        private string _md5;
        private string _extension;
        private DateTime _created;
        private DateTime _modified;
        private DateTime _lastaccess;
        private string _title;
        private List<string> _tags;

        public uint MetaVersion { get { return _metaversion; } }
        public uint DataVersion { get { return _dataversion; } }
        public string LockedBy { get { return _lockedby; } }
		public DateTime? LockedAt { get { return _lockedat; } }
		public string Creator { get { return _creator; } }
        public ETag ETag { get { return _etag; } }
        /// <summary>
        /// The length of the data asset in bytes
        /// </summary>
        public ulong Length { get { return _length; } }
        /// <summary>
        /// A MD5 Checksum of the data
        /// </summary>
        public string Md5 { get { return _md5; } }
        /// <summary>
        /// The extension of the file including the leading period (e.g., .exe, .txt, .docx, etc)
        /// </summary>
        public string Extension { get { return _extension; } }
        /// <summary>
        /// Gets the date/time that the asset was created on the server
        /// </summary>
        public DateTime Created { get { return _created; } }
        /// <summary>
        /// Gets the date/time that the asset was last modified on the server
        /// </summary>
        public DateTime Modified { get { return _modified; } }
        /// <summary>
        /// Gets the date.time that the asset was last accessed on the server
        /// </summary>
        public DateTime LastAccess { get { return _lastaccess; } }
        /// <summary>
        /// A descriptive title of the asset
        /// </summary>
        public string Title { get { return _title; } }
        /// <summary>
        /// A collection of tags identifying the asset
        /// </summary>
        public List<string> Tags { get { return _tags; } }

        /// <summary>
        /// User defined propertes
        /// </summary>
        public Dictionary<string, object> UserProperties;

        #endregion


        public FileSystem.MetaResource Resource
        {
            get { return (FileSystem.MetaResource)_resource; }
        }

        public bool IsLocked { get { return _lockedat != null && !string.IsNullOrEmpty(_lockedby); } }
        
        public MetaAsset(Logger logger)
            : base(logger)
        {
            _etag = new ETag("0");
        }

        public MetaAsset(Guid guid, FileSystem.IO fileSystem, Logger logger)
            : base(guid, AssetType.Meta, logger)
        {
            _resource = new FileSystem.MetaResource(guid, fileSystem, logger);
            _etag = new ETag("0");
            _tags = new List<string>();
            UserProperties = new Dictionary<string, object>();

            _state = new AssetState() { State = AssetState.Flags.CanTransfer };
        }

        public static MetaAsset Create(Guid guid, ETag etag, uint metaversion,
            uint dataversion, string lockedby, DateTime? lockedat, string creator, ulong length, string md5, 
            string extension, DateTime created, DateTime modified, DateTime lastaccess, string title, 
            List<string> tags, Dictionary<string, object> userproperties, FileSystem.IO fileSystem,
            Logger logger)
		{
            MetaAsset ma = new MetaAsset(guid, fileSystem, logger);
            ma._etag = etag;
            ma._metaversion = metaversion;
            ma._dataversion = dataversion;
            ma._lockedby = lockedby;
			ma._lockedat = lockedat;
			ma._creator = creator;
            ma._length = length;
            ma._md5 = md5;
            ma._extension = extension;
            ma._created = created;
            ma._modified = modified;
            ma._lastaccess = lastaccess;
            ma._title = title;

            for (int i = 0; i < tags.Count; i++)
                ma.Tags.Add(tags[i]);

            Dictionary<string, object>.Enumerator en = userproperties.GetEnumerator();
            while(en.MoveNext())
                ma.UserProperties.Add(en.Current.Key, en.Current.Value);

            return ma;
        }

        public static MetaAsset Create(NetworkPackage.MetaAsset netMa, FileSystem.IO fileSystem, Logger logger)
        {
            MetaAsset ma = new MetaAsset(logger);

            ma.ImportFromNetworkRepresentation(netMa);
            ma.AssignResource(fileSystem);

            return ma;
        }

        /// <summary>
        /// Assigns a FileSystem.MetaResource to this object - should be used when State is None
        /// </summary>
        /// <param name="fileSystem"></param>
        public void AssignResource(FileSystem.IO fileSystem)
        {
            _resource = new FileSystem.MetaResource(_guid, fileSystem, _logger);
        }

        public static MetaAsset Load(Guid guid, FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
        {
            MetaAsset ma = new MetaAsset(guid, fileSystem, generalLogger);

            if (!ma.Load(generalLogger))
                return null;

            return ma;
        }

        public void UpdateByServer(ETag etag, uint metaversion, uint dataversion, string lockedby, DateTime? lockedat, 
            string creator, ulong length, string md5, DateTime? created, DateTime modified, DateTime lastaccess)
        {
            _etag = etag;
            _metaversion = metaversion;
            _dataversion = dataversion;
            if(string.IsNullOrEmpty(lockedby)) _lockedby = lockedby;
            if(lockedat.HasValue) _lockedat = lockedat.Value;
            _creator = creator;
            _length = length;
            _md5 = md5;
            if(created.HasValue) _created = created.Value;
            _modified = modified;
            _lastaccess = lastaccess;
        }

        public void UpdateByUser(string title, List<string> tags)
        {
            _title = title;
            _tags = tags;
        }

        #region Remote Communications Code

        public ETag GetETagFromServer(Work.AssetJobBase job, Logger networkLogger)
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
                return null;
            }

            if (msg.State.Response.Headers["ETag"] == null)
                return null;

            etag = new ETag(msg.State.Response.Headers["ETag"].Replace("\"", ""));

            if (_logger != null)
            {
                _logger.Write(Logger.LevelEnum.Debug, "ETag received on " +
                    job.Id.ToString() + " for " + GuidString +
                    " with value of " + etag.Value);
            }

            msg.State.Dispose();

            return etag;
        }
        
        public static MetaAsset DownloadFromServer(string guid, FileSystem.IO fileSystem,
            Logger generalLogger, Logger networkLogger)
        {
            return DownloadFromServer(new Guid(guid), fileSystem, generalLogger, networkLogger);
        }

        public static MetaAsset DownloadFromServer(Guid guid, FileSystem.IO fileSystem,
            Logger generalLogger, Logger networkLogger)
        {
            MetaAsset ma = new MetaAsset(guid, fileSystem, generalLogger);

            if (ma.DownloadFromServer(networkLogger))
                return ma;

            return null;
        }

        public bool DownloadFromServer(Logger networkLogger)
        {
            if (!_state.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(_state, "Cannot download");

            Network.Message msg;
            NetworkPackage.MetaAsset networkMetaAsset;

            try
            {
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

            // Deserialize
            networkMetaAsset = new NetworkPackage.MetaAsset();
            try
            {
                networkMetaAsset.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while calling " +
                        "NetworkPackage.MetaAsset.Deserialize(), the exception follows:\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            // Dispose network assets
            msg.State.Dispose();

            // Import it to this object
            ImportFromNetworkRepresentation(networkMetaAsset);

            _state.State = AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.LoadedFromRemote;

            try
            {
                Save();
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to save the meta asset to the file system.\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            return true;
        }

        public bool DownloadFromServer(Work.AssetJobBase job, Logger networkLogger)
        {
            if (!_state.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(_state, "Cannot download");

            Network.Message msg;
            NetworkPackage.MetaAsset networkMetaAsset;

            try
            {
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

            // Deserialize
            networkMetaAsset = new NetworkPackage.MetaAsset();
            try
            {
                networkMetaAsset.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while calling " +
                        "NetworkPackage.MetaAsset.Deserialize(), the exception follows:\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            // Dispose network assets
            msg.State.Dispose();

            // Import it to this object
            ImportFromNetworkRepresentation(networkMetaAsset);
            
            _state.State = AssetState.Flags.CanTransfer | 
                AssetState.Flags.InMemory | 
                AssetState.Flags.LoadedFromRemote;

            try
            {
                Save();
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to save the meta asset to the file system.\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            return true;
        }

        public NetworkPackage.ServerResponse SaveToServer(Work.AssetJobBase job, Logger networkLogger)
        {
            NetworkPackage.ServerResponse sr;
            Network.Message msg;
            FileSystem.IOStream iostream;

            try
            {
                iostream = _resource.GetExclusiveReadStream("Common.Data.MetaAsset.SaveToServer()");
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource.\r\n" + Logger.ExceptionToString(e));
                throw e;
            }

            try
            {
                msg = new Network.Message(ServerSettings.Instance.ServerIp, ServerSettings.Instance.ServerPort,
                    _assetType.VirtualPath, _guid, _assetType, Network.OperationType.PUT, Network.DataStreamMethod.Stream,
                    iostream.Stream, null, iostream.Stream.Length, null, false, false, false, false,
                    ServerSettings.Instance.NetworkBufferSize, ServerSettings.Instance.NetworkTimeout,
                    _logger, networkLogger);
            }
            catch (Exception e)
            {
                _resource.CloseStream();

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
                _resource.CloseStream();

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

        #endregion

        #region Local IO Code

        public bool Load(Work.AssetJobBase job)
        {
            NetworkPackage.MetaAsset networkMetaAsset;

            if (job.AbortAction) return false;

            networkMetaAsset = new NetworkPackage.MetaAsset();

            // Read the file from the local store
            if (!networkMetaAsset.Read(Resource, _logger))
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "Failed to load the meta asset.");

                return false;
            }

            // Import it to this object
            ImportFromNetworkRepresentation(networkMetaAsset);

            _state.State = AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.LoadedFromLocal |
                AssetState.Flags.OnDisk;

            return true;
        }

        public bool Load(Logger generalLogger)
        {
            NetworkPackage.MetaAsset networkMetaAsset;

            networkMetaAsset = new NetworkPackage.MetaAsset();

            // Read the file from the local store
            if (!networkMetaAsset.Read(Resource, _logger))
            {
                if (generalLogger != null)
                    generalLogger.Write(Logger.LevelEnum.Normal, "Failed to load the meta asset.");

                return false;
            }

            // Import it to this object
            ImportFromNetworkRepresentation(networkMetaAsset);

            _state.State = AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.LoadedFromLocal |
                AssetState.Flags.OnDisk;

            return true;
        }

        public void Save()
        {
            NetworkPackage.MetaAsset nma = new NetworkPackage.MetaAsset();

            try
            {
                nma = ExportToNetworkRepresentation();
            }
            catch(Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to export a serializable representation of the meta asset.\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            try
            {
                nma.Save((FileSystem.MetaResource)_resource, _logger, true);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to save the meta asset to the file system.\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            _state.State |= AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.OnDisk;
        }

        public void SaveUsingVersionScheme()
        {
            NetworkPackage.MetaAsset nma = new NetworkPackage.MetaAsset();


            nma = ExportToNetworkRepresentation();

            try
            {
                nma.SaveUsingVersionScheme(_metaversion,
                    (FileSystem.MetaResource)_resource, _logger, true);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to save the meta asset to the file system.\r\n" +
                        Logger.ExceptionToString(e));
                throw e;
            }

            _state.State |= AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory;
        }

        public bool CopyCurrentToVersionScheme()
        {
            return _resource.CopyCurrentToVersionScheme(_metaversion, 
                "Common.Data.MetaAsset.CopyCurrentToVersionScheme()");
        }

        #endregion

        /// <summary>
        /// Exports this instance to an instance of Common.Network.MetaAsset to be transported to the server
        /// </summary>
        /// <returns></returns>
        public NetworkPackage.MetaAsset ExportToNetworkRepresentation()
        {
            // Do error checking
            if (GuidString == System.Guid.Empty.ToString("N"))
                throw new Exception("Guid cannot be empty");
            if (Length <= 0)
                throw new Exception("Length must be a positive value");
            if (string.IsNullOrEmpty(Extension))
                throw new Exception("Extension cannot be null or empty");
            if (string.IsNullOrEmpty(Title))
                throw new Exception("Title cannot be null or empty");
            if (Tags == null || Tags.Count <= 0)
                throw new Exception("Tags cannot be null or empty");



            Dictionary<string, object>.Enumerator en;
            NetworkPackage.MetaAsset ma = new NetworkPackage.MetaAsset();

            ma.Add("$guid", this.Guid);
            ma.Add("$etag", this.ETag.Value);
            ma.Add("$metaversion", this.MetaVersion);
            ma.Add("$dataversion", this.DataVersion);
            ma.Add("$lockedby", this.LockedBy);
			ma.Add("$lockedat", this.LockedAt);
			ma.Add("$creator", this.Creator);
            ma.Add("$length", this.Length);
            ma.Add("$md5", this.Md5);
            ma.Add("$extension", this.Extension);
            ma.Add("$created", this.Created);
            ma.Add("$modified", this.Modified);
            ma.Add("$lastaccess", this.LastAccess);
            ma.Add("$title", this.Title);
            ma.Add("$tags", this.Tags);

            en = UserProperties.GetEnumerator();
            while(en.MoveNext())
            {
                if (en.Current.Key.StartsWith("$"))
                    throw new Exception("Properties cannot begin with the '$' character as it is reserved.");
                ma.Add(en.Current.Key, en.Current.Value);
            }

            _state.State |= AssetState.Flags.InMemory |
                AssetState.Flags.CanTransfer;

            return ma;
        }

        /// <summary>
        /// Imports an instance of Common.Network.MetaAsset to this instance
        /// </summary>
        /// <param name="ma">Instance to import</param>
        public void ImportFromNetworkRepresentation(NetworkPackage.MetaAsset ma)
        {
            if (!ma.ContainsKey("$guid")) throw new Exception("The required property '$guid' does not exist.");
            if (!ma.ContainsKey("$etag")) throw new Exception("The required property '$etag' does not exist.");
            if (!ma.ContainsKey("$metaversion")) throw new Exception("The required property '$metaversion' does not exist.");
            if (!ma.ContainsKey("$dataversion")) throw new Exception("The required property '$dataversion' does not exist.");
            if (!ma.ContainsKey("$creator")) throw new Exception("The required property '$creator' does not exist.");
            if (!ma.ContainsKey("$length")) throw new Exception("The required property '$length' does not exist.");
            if (!ma.ContainsKey("$md5")) throw new Exception("The required property '$md5' does not exist.");
            if (!ma.ContainsKey("$extension")) throw new Exception("The required property '$extension' does not exist.");
            if (!ma.ContainsKey("$created")) throw new Exception("The required property '$created' does not exist.");
            if (!ma.ContainsKey("$modified")) throw new Exception("The required property '$modified' does not exist.");
            if (!ma.ContainsKey("$lastaccess")) throw new Exception("The required property '$lastaccess' does not exist.");
            if (!ma.ContainsKey("$title")) throw new Exception("The required property '$title' does not exist.");
            if (!ma.ContainsKey("$tags")) throw new Exception("The required property '$tags' does not exist.");

            if (ma["$guid"].GetType() != typeof(Guid)) throw new Exception("The required property '$guid' must be of type string.");
            if (ma["$etag"].GetType() != typeof(string)) throw new Exception("The required property '$etag' must be of type string.");
            if (ma["$metaversion"].GetType() != typeof(uint)) throw new Exception("The required property '$metaversion' must be of type uint.");
            if (ma["$dataversion"].GetType() != typeof(uint)) throw new Exception("The required property '$dataversion' must be of type uint.");
            if (ma.ContainsKey("$lockedby")) 
            {
                if ((string)ma["$lockedby"] != default(string) && 
                    ma["$lockedby"].GetType() != typeof(string))
                    throw new Exception("The property '$lockedby' must be of type string."); 
            }
            if (ma.ContainsKey("$lockedat")) 
            {
                if (ma["$lockedat"] != null && 
                    ma["$lockedat"].GetType() != typeof(DateTime))
                    throw new Exception("The property '$lockedat' must be of type DateTime."); 
            }
            if (ma["$creator"].GetType() != typeof(string)) throw new Exception("The required property '$creator' must be of type string.");
            if (ma.ContainsKey("$previousversion")) { if (ma["$previousversion"].GetType() != typeof(string)) throw new Exception("The property '$previousversion' must be of type string."); }
			if (ma.ContainsKey("$nextversion")) { if (ma["$nextversion"].GetType() != typeof(string)) throw new Exception("The property '$nextversion' must be of type string."); }
            if (ma["$length"].GetType() != typeof(ulong)) throw new Exception("The required property '$length' must be of type ulong.");
            if (ma["$md5"].GetType() != typeof(string)) throw new Exception("The required property '$md5' must be of type string.");
            if (ma["$extension"].GetType() != typeof(string)) throw new Exception("The required property '$extension' must be of type string.");
            if (ma["$created"].GetType() != typeof(DateTime)) throw new Exception("The required property '$created' must be of type DateTime.");
            if (ma["$modified"].GetType() != typeof(DateTime)) throw new Exception("The required property '$modified' must be of type DateTime.");
            if (ma["$lastaccess"].GetType() != typeof(DateTime)) throw new Exception("The required property '$lastaccess' must be of type DateTime.");
            if (ma["$title"].GetType() != typeof(string)) throw new Exception("The required property '$title' must be of type string.");
            if (ma["$tags"].GetType() != typeof(string[])) throw new Exception("The required property '$tags' must be of type string[].");



            _guid = (Guid)ma["$guid"];
            _etag = new ETag((string)ma["$etag"]);
            _metaversion = (uint)ma["$metaversion"];
            _dataversion = (uint)ma["$dataversion"];
			if (ma.ContainsKey("$lockedby")) _lockedby = (string)ma["$lockedby"];
            if (ma.ContainsKey("$lockedat"))
            {
                if (ma["$lockedat"] == null)
                    _lockedat = null;
                else
                    _lockedat = (DateTime)ma["$lockedat"];
            }
			_creator = (string)ma["$creator"];
            _length = Convert.ToUInt64(ma["$length"]);
            _md5 = (string)ma["$md5"];
            _extension = (string)ma["$extension"];
            _created = (DateTime)ma["$created"];
            _modified = (DateTime)ma["$modified"];
            _lastaccess = (DateTime)ma["$lastaccess"];
            _title = (string)ma["$title"];
            _tags = new List<string>();
            string[] tagsAsStrings = (string[])ma["$tags"];
            for (int i = 0; i < tagsAsStrings.Length; i++)
            {
                _tags.Add(tagsAsStrings[i]);
            }

            ma.Remove("$guid");
            ma.Remove("$etag");
            ma.Remove("$metaversion");
            ma.Remove("$dataversion");
            if (ma.ContainsKey("$lockedby")) ma.Remove("$lockedby");
            if (ma.ContainsKey("$lockedat")) ma.Remove("$lockedat");
            ma.Remove("$creator");
            if (ma.ContainsKey("$previousversion")) ma.Remove("$previousversion");
            if (ma.ContainsKey("$nextversion")) ma.Remove("$nextversion");
            ma.Remove("$length");
            ma.Remove("$md5");
            ma.Remove("$extension");
            ma.Remove("$created");
            ma.Remove("$modified");
            ma.Remove("$lastaccess");
            ma.Remove("$title");
            ma.Remove("$tags");

            IEnumerator<Common.NetworkPackage.DictionaryEntry<string, object>> en = 
                (IEnumerator<Common.NetworkPackage.DictionaryEntry<string, object>>)ma.GetEnumerator();
            
            if (UserProperties == null)
                UserProperties = new Dictionary<string, object>();
            else
                UserProperties.Clear();

            while (en.MoveNext())
                UserProperties.Add(en.Current.Key, en.Current.Value);

            _state.State |= AssetState.Flags.InMemory |
                AssetState.Flags.CanTransfer;
        }

        public void SetMetaVersion(uint value)
        {
            _metaversion = value;
        }

        public void SetDataVersion(uint value)
        {
            _dataversion = value;
        }

        public void ApplyLock(DateTime at, string by)
        {
            _lockedat = at;
            _lockedby = by;
        }

        public void ReleaseLock()
        {
            _lockedat = null;
            _lockedby = null;
        }
    }
}
