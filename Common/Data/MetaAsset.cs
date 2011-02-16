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
using System.IO;
using System.Collections.Generic;

namespace Common.Data
{
    /// <summary>
    /// A <see cref="MetaAsset"/> represents a collection of meta information in a way 
    /// that it is usable by the OpenDMS.NET Project.
    /// </summary>
    public sealed class MetaAsset 
        : AssetBase
    {
        #region Meta Data - The following are mandatory properties for metadata

        /// <summary>
        /// Represents the <see cref="ETag"/> of this <see cref="MetaAsset"/>.
        /// </summary>
        private ETag _etag;
        /// <summary>
        /// A numeric value indicating the version number of this <see cref="MetaAsset"/>.
        /// </summary>
        private uint _metaversion;
        /// <summary>
        /// A numeric value indicating the version number of the <see cref="DataAsset"/> paired with this <see cref="MetaAsset"/>.
        /// </summary>
        private uint _dataversion;
        /// <summary>
        /// The username of the user holding the outstanding lock.
        /// </summary>
        private string _lockedby;
        /// <summary>
        /// A timestamp of when the asset was locked, if null then not locked.
        /// </summary>
        private DateTime? _lockedat;
        /// <summary>
        /// The creator of this asset.
        /// </summary>
        /// <remarks>Creator applies only to this version of the asset.  Different versions will potentially have different creators.</remarks>
		private string _creator;
        /// <summary>
        /// The quantity of bytes of the <see cref="DataAsset"/>.
        /// </summary>
        private ulong _length;
        /// <summary>
        /// A checksum value of the <see cref="DataAsset"/>.
        /// </summary>
        private string _md5;
        /// <summary>
        /// The extension used by the <see cref="DataAsset"/> including the leading period (e.g., .exe, .txt, .docx, etc).
        /// </summary>
        private string _extension;
        /// <summary>
        /// A timestamp marking the creation of this asset on the server.
        /// </summary>
        /// <remarks>The timestamp marks the date of creation of this verison.</remarks>
        private DateTime _created;
        /// <summary>
        /// A timestamp marking the modification of this asset on the server.
        /// </summary>
        /// <remarks>The timestamp marks the modification of this version (rare as any change by a client should result in a new version, 
        /// but system administrators should have the ability to modify without creating a new version.</remarks>
        private DateTime _modified;
        /// <summary>
        /// A timestamp marking when this asset was last accessed on the server.
        /// </summary>
        /// <remarks>The timestamp applies to this version.</remarks>
        private DateTime _lastaccess;
        /// <summary>
        /// A descriptive title of this asset.
        /// </summary>
        private string _title;
        /// <summary>
        /// A collection of strings that are descriptive of the document.  These are primarily used for searching.
        /// </summary>
        private List<string> _tags;

        /// <summary>
        /// Gets a numeric value indicating the version number of this <see cref="MetaAsset"/>.
        /// </summary>
        public uint MetaVersion { get { return _metaversion; } }
        /// <summary>
        /// Gets a numeric value indicating the version number of the <see cref="DataAsset"/> paired with this <see cref="MetaAsset"/>.
        /// </summary>
        public uint DataVersion { get { return _dataversion; } }
        /// <summary>
        /// Gets the username of the user holding the outstanding lock.
        /// </summary>
        public string LockedBy { get { return _lockedby; } }
        /// <summary>
        /// Gets a timestamp of when the asset was locked, if null then not locked.
        /// </summary>
		public DateTime? LockedAt { get { return _lockedat; } }
        /// <summary>
        /// Gets the creator of this asset.
        /// </summary>
        /// <remarks>Creator applies only to this version of the asset.  Different versions will potentially have different creators.</remarks>
		public string Creator { get { return _creator; } }
        /// <summary>
        /// Gets the <see cref="ETag"/> of this <see cref="MetaAsset"/>.
        /// </summary>
        public ETag ETag { get { return _etag; } }
        /// <summary>
        /// Gets the quantity of bytes of the <see cref="DataAsset"/>.
        /// </summary>
        public ulong Length { get { return _length; } }
        /// <summary>
        /// Gets a checksum value of the <see cref="DataAsset"/>.
        /// </summary>
        public string Md5 { get { return _md5; } }
        /// <summary>
        /// Gets the extension used by the <see cref="DataAsset"/> including the leading period (e.g., .exe, .txt, .docx, etc).
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
        /// Gets the date/time that the asset was last accessed on the server
        /// </summary>
        public DateTime LastAccess { get { return _lastaccess; } }
        /// <summary>
        /// A descriptive title of this asset
        /// </summary>
        public string Title { get { return _title; } }
        /// <summary>
        /// A collection of strings that are descriptive of the document.  These are primarily used for searching.
        /// </summary>
        public List<string> Tags { get { return _tags; } }

        /// <summary>
        /// User defined propertes Key is the name or title of the property, object is the value of the property.
        /// </summary>
        public Dictionary<string, object> UserProperties { get; set; }

        #endregion


        /// <summary>
        /// Gets a reference to the <see cref="FileSystem.MetaResource"/> giving this <see cref="MetaAsset"/> access to the file system.
        /// </summary>
        public FileSystem.MetaResource Resource
        {
            get { return (FileSystem.MetaResource)_resource; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is locked; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocked { get { return _lockedat != null && !string.IsNullOrEmpty(_lockedby); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaAsset"/> class.
        /// </summary>
        public MetaAsset()
            : base()
        {
            _etag = new ETag("0");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaAsset"/> class.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> providing a unique reference to the Asset.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        public MetaAsset(Guid guid, FileSystem.IO fileSystem)
            : base(guid, AssetType.Meta)
        {
            _resource = new FileSystem.MetaResource(guid, fileSystem);
            _etag = new ETag("0");
            _tags = new List<string>();
            UserProperties = new Dictionary<string, object>();

            _state = new AssetState() { State = AssetState.Flags.CanTransfer };
        }

        /// <summary>
        /// Creates an instance of a <see cref="MetaAsset"/> object.
        /// </summary>
        /// <param name="guid">A Guid that provides a unique reference to an Asset.</param>
        /// <param name="etag">Represents the <see cref="ETag"/> of this <see cref="MetaAsset"/>.</param>
        /// <param name="metaversion">A numeric value indicating the version number of this <see cref="MetaAsset"/>.</param>
        /// <param name="dataversion">A numeric value indicating the version number of the <see cref="DataAsset"/> paired with this <see cref="MetaAsset"/>.</param>
        /// <param name="lockedby">The username of the user holding the outstanding lock.</param>
        /// <param name="lockedat">A timestamp of when the asset was locked, if null then not locked.</param>
        /// <param name="creator">The creator of this asset.</param>
        /// <param name="length">The quantity of bytes of the <see cref="DataAsset"/>.</param>
        /// <param name="md5">A checksum value of the <see cref="DataAsset"/>.</param>
        /// <param name="extension">The extension used by the <see cref="DataAsset"/> including the leading period (e.g., .exe, .txt, .docx, etc).</param>
        /// <param name="created">A timestamp marking the creation of this asset on the server.</param>
        /// <param name="modified">A timestamp marking the modification of this asset on the server.</param>
        /// <param name="lastaccess">A timestamp marking when this asset was last accessed on the server.</param>
        /// <param name="title">A descriptive title of this asset.</param>
        /// <param name="tags">A collection of strings that are descriptive of the document.  These are primarily used for searching.</param>
        /// <param name="userproperties">User defined propertes Key is the name or title of the property, object is the value of the property.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public static MetaAsset Create(Guid guid, ETag etag, uint metaversion,
            uint dataversion, string lockedby, DateTime? lockedat, string creator, ulong length, string md5, 
            string extension, DateTime created, DateTime modified, DateTime lastaccess, string title, 
            List<string> tags, Dictionary<string, object> userproperties, FileSystem.IO fileSystem)
		{
            MetaAsset ma = new MetaAsset(guid, fileSystem);
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

        /// <summary>
        /// Creates an instance of a <see cref="MetaAsset"/> object.
        /// </summary>
        /// <param name="netMa">The <see cref="NetworkPackage.MetaAsset"/> object to use as the basis for the new <see cref="MetaAsset"/> instance.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public static MetaAsset Create(NetworkPackage.MetaAsset netMa, FileSystem.IO fileSystem)
        {
            MetaAsset ma = new MetaAsset();

            ma.ImportFromNetworkRepresentation(netMa);
            ma.AssignResource(fileSystem);

            return ma;
        }

        /// <summary>
        /// Copies this instance to a new <see cref="MetaAsset"/> using the specified new Guid.
        /// </summary>
        /// <param name="newGuid">The new Guid.</param>
        /// <param name="saveToFS">if set to <c>true</c> then the new <see cref="MetaAsset"/> is saved to the file system before being returned.</param>
        /// <param name="fileSystem">A reference to the file system.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public MetaAsset CopyToNew(Guid newGuid, bool saveToFS, FileSystem.IO fileSystem)
        {
            Dictionary<string, object>.Enumerator en;
            Dictionary<string, object> uprop = new Dictionary<string, object>();
            MetaAsset newMa;
            
            en = this.UserProperties.GetEnumerator();
            while(en.MoveNext())
                uprop.Add(en.Current.Key, en.Current.Value);

            newMa = MetaAsset.Create(newGuid, this.ETag, this.MetaVersion, this.DataVersion, this.LockedBy,
                this.LockedAt, this.Creator, this.Length, this.Md5, this.Extension, this.Created, this.Modified,
                this.LastAccess, this.Title, this.Tags, uprop, fileSystem);

            if (saveToFS) newMa.Save();

            return newMa;
        }

        /// <summary>
        /// Assigns a FileSystem.MetaResource to this object - should be used when State is None
        /// </summary>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <example>
        /// This sample shows how to use the <see cref="AssignResource"/> method.
        /// <code>
        /// // This assumes the fileSystem argument is properly instantiated.
        /// void A(FileSystem.IO fileSystem)
        /// {
        ///     // We discover that this MetaAsset has a "None" flag set for state.
        ///     // This means that the MetaAsset cannot do anything which just will not work!
        ///     // The remedy is to tell this library to assign the resource
        ///     // The library will determine how to handle this, just tell it to make the assignment
        ///     if (this.AssetState.State == Data.AssetState.Flags.None)
        ///         AssignResource(fileSystem);
        /// }
        /// </code>
        /// </example>
        public void AssignResource(FileSystem.IO fileSystem)
        {
            _resource = new FileSystem.MetaResource(_guid, fileSystem);
        }

        /// <summary>
        /// Instantiates a <see cref="MetaAsset"/> object based on the corresponding <see cref="FileSystem.MetaResource"/>.
        /// </summary>
        /// <param name="guid">A Guid that provides a unique reference to an Asset.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public static MetaAsset Load(Guid guid, FileSystem.IO fileSystem)
        {
            MetaAsset ma = new MetaAsset(guid, fileSystem);

            if (!ma.Load())
                return null;

            return ma;
        }

        /// <summary>
        /// Updates this <see cref="MetaAsset"/>.  This should be used only when the update is being requested by the server.
        /// </summary>
        /// <param name="etag">Represents the <see cref="ETag"/> of this <see cref="MetaAsset"/>.</param>
        /// <param name="metaversion">A numeric value indicating the version number of this <see cref="MetaAsset"/>.</param>
        /// <param name="dataversion">A numeric value indicating the version number of the <see cref="DataAsset"/> paired with this <see cref="MetaAsset"/>.</param>
        /// <param name="lockedby">The username of the user holding the outstanding lock.</param>
        /// <param name="lockedat">A timestamp of when the asset was locked, if null then not locked.</param>
        /// <param name="creator">The creator of this asset.</param>
        /// <param name="length">The quantity of bytes of the <see cref="DataAsset"/>.</param>
        /// <param name="md5">A checksum value of the <see cref="DataAsset"/>.</param>
        /// <param name="created">A timestamp marking the creation of this asset on the server.</param>
        /// <param name="modified">A timestamp marking the modification of this asset on the server.</param>
        /// <param name="lastaccess">A timestamp marking when this asset was last accessed on the server.</param>
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

        /// <summary>
        /// Updates this <see cref="MetaAsset"/>.  This should be used only when the update is being requested by the user.
        /// </summary>
        /// <param name="title">A descriptive title of this asset.</param>
        /// <param name="tags">A collection of strings that are descriptive of the document.  These are primarily used for searching.</param>
        public void UpdateByUser(string title, List<string> tags)
        {
            _title = title;
            _tags = tags;
        }

        /// <summary>
        /// Updates this <see cref="MetaAsset"/>.  This should be used only when the update is being requested by the user.
        /// </summary>
        /// <param name="propertiesToUpdate">A collection of properties to update.  The key is the property name and the value is the value of the property.</param>
        public void UpdateByUser(Dictionary<string, object> propertiesToUpdate)
        {
            Dictionary<string, object>.Enumerator en;
            string title = null;
            List<string> tags = null;

            // Check properties for those that are not allowed to be changed by the user
            if (propertiesToUpdate.ContainsKey("$guid"))
                throw new ArgumentException("A user cannot modify the resource's guid property.");
            if (propertiesToUpdate.ContainsKey("$etag"))
                throw new ArgumentException("A user cannot modify the resource's etag property.");
            if (propertiesToUpdate.ContainsKey("$metaversion"))
                throw new ArgumentException("A user cannot modify the resource's metaversion property.");
            if (propertiesToUpdate.ContainsKey("$dataversion"))
                throw new ArgumentException("A user cannot modify the resource's dataversion property.");
            if (propertiesToUpdate.ContainsKey("$lockedby"))
                throw new ArgumentException("A user cannot modify the resource's lockedby property.");
            if (propertiesToUpdate.ContainsKey("$lockedat"))
                throw new ArgumentException("A user cannot modify the resource's lockedat property.");
            if (propertiesToUpdate.ContainsKey("$creator"))
                throw new ArgumentException("A user cannot modify the resource's creator property.");
            if (propertiesToUpdate.ContainsKey("$length"))
                throw new ArgumentException("A user cannot modify the resource's length property.");
            if (propertiesToUpdate.ContainsKey("$md5"))
                throw new ArgumentException("A user cannot modify the resource's md5 property.");
            if (propertiesToUpdate.ContainsKey("$extension"))
                throw new ArgumentException("A user cannot modify the resource's extension property.");
            if (propertiesToUpdate.ContainsKey("$created"))
                throw new ArgumentException("A user cannot modify the resource's created property.");
            if (propertiesToUpdate.ContainsKey("$modified"))
                throw new ArgumentException("A user cannot modify the resource's modified property.");
            if (propertiesToUpdate.ContainsKey("$lastaccess"))
                throw new ArgumentException("A user cannot modify the resource's lastaccess property.");

            if (propertiesToUpdate.ContainsKey("$title"))
            {
                if (propertiesToUpdate["$title"].GetType() != typeof(string))
                    throw new ArgumentException("The property $title must be of type string.");
                if (((string)propertiesToUpdate["$title"]).Trim().Length <= 0)
                    throw new ArgumentException("The property $title cannot be empty.");
                title = (string)propertiesToUpdate["$title"];
                propertiesToUpdate.Remove("$title");
            }

            if (propertiesToUpdate.ContainsKey("$tags"))
            {
                if (propertiesToUpdate["$tags"].GetType() != typeof(List<string>))
                    throw new ArgumentException("The property $tags must be of type List<string>.");
                if (((List<string>)propertiesToUpdate["$tags"]).Count <= 0)
                    throw new ArgumentException("The property $tags must have at least one entry.");
                tags = (List<string>)propertiesToUpdate["$tags"];
                propertiesToUpdate.Remove("$tags");
            }

            en = propertiesToUpdate.GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current.Key.StartsWith("$"))
                    throw new ArgumentException("User properties cannot start with the $ character as it is reserved.");
            }

            // If we get here, checking has been successful, thus we update
            if (title != null) _title = title;
            if (tags != null) _tags = tags;

            en = propertiesToUpdate.GetEnumerator();

            while (en.MoveNext())
            {
                if (UserProperties.ContainsKey(en.Current.Key))
                    UserProperties[en.Current.Key] = en.Current.Value;
                else
                    UserProperties.Add(en.Current.Key, en.Current.Value);
            }           
        }

        #region Remote Communications Code

        /// <summary>
        /// Gets the current <see cref="Head"/> from the server.
        /// </summary>
        /// <param name="job">The <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <returns><see cref="Head"/> if successful, otherwise <c>null</c>.</returns>
        public Head GetHeadFromServer(Work.AssetJobBase job)
        {
            if (Data.AssetType.IsNullOrUnknown(_assetType))
                throw new InvalidOperationException();

            string md5;
            ETag etag;
            Network.Message msg;

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    _assetType.VirtualPath, Guid, _assetType, Network.OperationType.HEAD,
                    Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while creating the network message.", e);
                throw e;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message.", e);
                throw e;
            }

            if (job.AbortAction)
            {
                msg.State.Dispose();
                return null;
            }

            if (msg.State.Response.Headers["ETag"] == null || 
                msg.State.Response.Headers["MD5"] == null)
                return null;

            etag = new ETag(msg.State.Response.Headers["ETag"].Replace("\"", ""));
            md5 = msg.State.Response.Headers["MD5"];

            Logger.Network.Debug("Header received on " + job.Id.ToString() + " for " + GuidString +
                    " with value of ETag: " + etag.Value + ", MD5: " + md5);

            msg.State.Dispose();

            return new Head(etag, md5);
        }

        /// <summary>
        /// Gets the current <see cref="ETag"/> from the server.
        /// </summary>
        /// <param name="job">The <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <returns><see cref="ETag"/> if successful, otherwise <c>null</c>.</returns>
        public ETag GetETagFromServer(Work.AssetJobBase job)
        {
            if (Data.AssetType.IsNullOrUnknown(_assetType))
                throw new InvalidOperationException();

            ETag etag;
            Network.Message msg;

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    _assetType.VirtualPath, Guid, _assetType, Network.OperationType.HEAD,
                    Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while creating the network message.", e);
                throw e;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message.", e);
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

            Logger.Network.Debug("ETag received on " + job.Id.ToString() + " for " + GuidString +
                    " with value of " + etag.Value);

            msg.State.Dispose();

            return etag;
        }

        /// <summary>
        /// Downloads the <see cref="MetaAsset"/> corresponding to the specified <see cref="Guid"/> from the server saving it to the local file system.
        /// </summary>
        /// <param name="guid">A Guid that provides a unique reference to an Asset.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public static MetaAsset DownloadFromServer(string guid, FileSystem.IO fileSystem)
        {
            return DownloadFromServer(new Guid(guid), fileSystem);
        }

        /// <summary>
        /// Downloads the <see cref="MetaAsset"/> corresponding to the specified <see cref="Guid"/> from the server saving it to the local file system.
        /// </summary>
        /// <param name="guid">A Guid that provides a unique reference to an Asset.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public static MetaAsset DownloadFromServer(Guid guid, FileSystem.IO fileSystem)
        {
            MetaAsset ma = new MetaAsset(guid, fileSystem);

            if (ma.DownloadFromServer())
                return ma;

            return null;
        }

        /// <summary>
        /// Downloads the <see cref="MetaAsset"/> corresponding to this local <see cref="MetaAsset"/> from the server overwriting it on the local file system.
        /// </summary>
        /// <returns><c>True</c> if successful; otherwise <c>false</c>.</returns>
        public bool DownloadFromServer()
        {
            if (!_state.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(_state, "Cannot download");

            Network.Message msg;
            NetworkPackage.MetaAsset networkMetaAsset;

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    _assetType.VirtualPath, Guid, _assetType, Network.OperationType.GET,
                    Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while creating the network message.", e);
                throw e;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message.", e);
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
                Logger.Network.Error("An exception occurred while calling NetworkPackage.ServerResponse.Deserialize().", e);
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
                Logger.General.Error("An exception occurred while attempting to save the meta asset to the file system.", e);
                throw e;
            }

            return true;
        }

        /// <summary>
        /// Downloads the <see cref="MetaAsset"/> corresponding to this local <see cref="MetaAsset"/> from the server overwriting it on the local file system.
        /// </summary>
        /// <param name="job">The <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <returns><c>True</c> if successful; otherwise <c>false</c>.</returns>
        public bool DownloadFromServer(Work.AssetJobBase job)
        {
            if (!_state.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(_state, "Cannot download");

            Network.Message msg;
            NetworkPackage.MetaAsset networkMetaAsset;

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    _assetType.VirtualPath, Guid, _assetType, Network.OperationType.GET,
                    Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while creating the network message.", e);
                throw e;
            }

            try
            {
                msg.Send();
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while sending the network message.", e);
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
                Logger.Network.Error("An exception occurred while calling NetworkPackage.MetaAsset.Deserialize().", e);
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
                Logger.General.Error("An exception occurred while attempting to save the meta asset to the file system.", e);
                throw e;
            }

            return true;
        }

        /// <summary>
        /// Saves this <see cref="MetaAsset"/> to a the server returning a <see cref="NetworkPackage.ServerResponse"/> representing the result.
        /// </summary>
        /// <param name="job">The <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <returns>A <see cref="NetworkPackage.ServerResponse"/> returned by the server.</returns>
        public NetworkPackage.ServerResponse SaveToServer(Work.AssetJobBase job)
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
                Logger.General.Error("An exception occurred while attempting to open a resource.", e);
                throw e;
            }

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    _assetType.VirtualPath, _guid, _assetType, Network.OperationType.PUT, Network.DataStreamMethod.Stream,
                    iostream.Stream, null, iostream.Stream.Length, null, false, false, false, false,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                _resource.CloseStream();
                Logger.Network.Error("An exception occurred while creating the network message.", e);
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
                Logger.Network.Error("An exception occurred while sending the network message.", e);
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
                Logger.Network.Error("An exception occurred while calling NetworkPackage.ServerResponse.Deserialize().", e);
                throw e;
            }

            return sr;
        }

        /// <summary>
        /// Creates this <see cref="MetaAsset"/> on the server returning a <see cref="NetworkPackage.ServerResponse"/> representing the result.
        /// </summary>
        /// <param name="job">The <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <returns>A <see cref="NetworkPackage.ServerResponse"/> returned by the server.</returns>
        public NetworkPackage.ServerResponse CreateOnServer(Work.AssetJobBase job)
        {
            NetworkPackage.ServerResponse sr;
            Network.Message msg;
            FileSystem.IOStream iostream;

            try
            {
                iostream = _resource.GetExclusiveReadStream("Common.Data.MetaAsset.CreateOnServer()");
            }
            catch (Exception e)
            {
                Logger.General.Error("An exception occurred while attempting to open a resource.", e);
                throw e;
            }

            try
            {
                msg = new Network.Message(SettingsBase.Instance.ServerIp, SettingsBase.Instance.ServerPort,
                    _assetType.VirtualPath, _guid, _assetType, Network.OperationType.POST, Network.DataStreamMethod.Stream,
                    iostream.Stream, null, iostream.Stream.Length, null, false, false, false, false,
                    SettingsBase.Instance.NetworkBufferSize, SettingsBase.Instance.NetworkTimeout);
            }
            catch (Exception e)
            {
                _resource.CloseStream();
                Logger.Network.Error("An exception occurred while creating the network message.", e);
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
                Logger.Network.Error("An exception occurred while sending the network message.", e);
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
                Logger.Network.Error("An exception occurred while calling NetworkPackage.ServerResponse.Deserialize().", e);
                throw e;
            }

            return sr;
        }

        #endregion

        #region Local IO Code

        /// <summary>
        /// Loads this <see cref="MetaAsset"/> using its <see cref="FileSystem.MetaResource"/>.
        /// </summary>
        /// <param name="job">The <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Load(Work.AssetJobBase job)
        {
            NetworkPackage.MetaAsset networkMetaAsset;

            if (job.AbortAction) return false;

            networkMetaAsset = new NetworkPackage.MetaAsset();

            // Read the file from the local store
            if (!networkMetaAsset.Read(Resource))
            {
                Logger.General.Error("Failed to load the meta asset.");
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

        /// <summary>
        /// Loads this <see cref="MetaAsset"/> using its <see cref="FileSystem.MetaResource"/>.
        /// </summary>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Load()
        {
            NetworkPackage.MetaAsset networkMetaAsset;

            networkMetaAsset = new NetworkPackage.MetaAsset();

            // Read the file from the local store
            try
            {
                if (!networkMetaAsset.Read(Resource))
                {
                    Logger.General.Error("Failed to load the meta asset.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to load the meta asset.", e);
                return false;
            }

            // Import it to this object
            try
            {
                ImportFromNetworkRepresentation(networkMetaAsset);
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to import the meta asset.", e);
                return false;
            }

            _state.State = AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.LoadedFromLocal |
                AssetState.Flags.OnDisk;

            return true;
        }

        /// <summary>
        /// Saves this <see cref="MetaAsset"/> to the file system.
        /// </summary>
        public void Save()
        {
            NetworkPackage.MetaAsset nma = new NetworkPackage.MetaAsset();

            try
            {
                nma = ExportToNetworkRepresentation();
            }
            catch(Exception e)
            {
                Logger.General.Error("An exception occurred while " +
                        "attempting to export a serializable representation of the meta asset.", e);
                throw e;
            }

            try
            {
                nma.Save((FileSystem.MetaResource)_resource, true);
            }
            catch (Exception e)
            {
                Logger.General.Error("An exception occurred while attempting to save the meta asset to the file system.", e);
                throw e;
            }

            _state.State |= AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.OnDisk;
        }

        /// <summary>
        /// Saves this <see cref="MetaAsset"/> to the file system using version scheme naming.
        /// </summary>
        public void SaveUsingVersionScheme()
        {
            NetworkPackage.MetaAsset nma = new NetworkPackage.MetaAsset();


            nma = ExportToNetworkRepresentation();

            try
            {
                nma.SaveUsingVersionScheme(_metaversion,
                    (FileSystem.MetaResource)_resource, true);
            }
            catch (Exception e)
            {
                Logger.General.Error("An exception occurred while attempting to save the meta asset to the file system.", e);
                throw e;
            }

            _state.State |= AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory;
        }

        /// <summary>
        /// Creates a copy of this <see cref="MetaAsset"/> on the file system using the version scheme naming.
        /// </summary>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool CopyCurrentToVersionScheme()
        {
            return _resource.CopyCurrentToVersionScheme(_metaversion, 
                "Common.Data.MetaAsset.CopyCurrentToVersionScheme()");
        }

        #endregion

        /// <summary>
        /// Exports this instance to an instance of <see cref="NetworkPackage.MetaAsset"/>.
        /// </summary>
        /// <returns>A <see cref="NetworkPackage.MetaAsset"/> representing this <see cref="MetaAsset"/>.</returns>
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
        /// Imports an instance of a <see cref="NetworkPackage.MetaAsset"/> to this instance.
        /// </summary>
        /// <param name="ma">The <see cref="NetworkPackage.MetaAsset"/> to import.</param>
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

        /// <summary>
        /// Sets the <see cref="MetaVersion"/> property.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetMetaVersion(uint value)
        {
            _metaversion = value;
        }

        /// <summary>
        /// Sets the <see cref="DataVersion"/> property.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetDataVersion(uint value)
        {
            _dataversion = value;
        }

        /// <summary>
        /// Sets the <see cref="Extension"/> property.
        /// </summary>
        /// <param name="ext">The value.</param>
        public void SetExtension(string ext)
        {
            _extension = ext;
        }

        /// <summary>
        /// Applies a lock to this <see cref="MetaAsset"/>.
        /// </summary>
        /// <param name="at">A timestamp when the lock was applied.</param>
        /// <param name="by">The username of the user that has the lock.</param>
        public void ApplyLock(DateTime at, string by)
        {
            _lockedat = at;
            _lockedby = by;
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public void ReleaseLock()
        {
            _lockedat = null;
            _lockedby = null;
        }
    }
}
