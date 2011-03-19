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
using Common.CouchDB;

namespace Common.Storage
{
    /// <summary>
    /// A <see cref="MetaAsset"/> represents a collection of meta information in a way 
    /// that it is usable by the OpenDMS.NET Project.
    /// </summary>
    public sealed class MetaAsset
        : AssetBase
    {
        private static const System.Collections.Generic.List<string> ReservedPropertyNames = InstantiateReservedProperties();

        private Document _doc;
        private System.Collections.Generic.Dictionary<string, object> _userProperties;

        /// <summary>
        /// Gets or sets the username of the user holding the outstanding lock.
        /// </summary>
        public string LockedBy
        {
            get { return (string)GetProperty("$lockedby"); }
            set { SetProperty("$lockedby", value); }
        }

        /// <summary>
        /// Gets or sets a timestamp of when the asset was locked, if null then not locked.
        /// </summary>
        public DateTime? LockedAt
        {
            get { return (DateTime?)GetProperty("$lockedat"); }
            set { SetProperty("$lockedat", value); }
        }

        /// <summary>
        /// Gets or sets the creator of this asset.
        /// </summary>
        /// <remarks>Creator applies only to this version of the asset.  Different versions will potentially have different creators.</remarks>
        public string Creator
        {
            get { return (string)GetProperty("$creator"); }
            set { SetProperty("$creator", value); }
        }

        /// <summary>
        /// Gets or sets The quantity of bytes of the <see cref="DataAsset"/>.
        /// </summary>
        public ulong Length
        {
            get { return (ulong)GetProperty("$length"); }
            set { SetProperty("$length", value); }
        }

        /// <summary>
        /// Gets or sets a checksum value of the <see cref="DataAsset"/>.
        /// </summary>
        public string Md5
        {
            get { return (string)GetProperty("$md5"); }
            set { SetProperty("$md5", value); }
        }

        /// <summary>
        /// Gets or sets the extension used by the <see cref="DataAsset"/> including the leading period (e.g., .exe, .txt, .docx, etc).
        /// </summary>
        public string Extension
        {
            get { return (string)GetProperty("$extension"); }
            set { SetProperty("$extension", value); }
        }

        /// <summary>
        /// Gets or sets the date/time that the asset was created on the server
        /// </summary>
        public DateTime Created
        {
            get { return (DateTime)GetProperty("$created"); }
            set { SetProperty("$created", value); }
        }

        /// <summary>
        /// Gets or sets the date/time that the asset was last accessed on the server
        /// </summary>
        public DateTime LastAccess
        {
            get { return (DateTime)GetProperty("$created"); }
            set { SetProperty("$created", value); }
        }

        /// <summary>
        /// Gets or sets a descriptive title of this asset
        /// </summary>
        public string Title
        {
            get { return (string)GetProperty("$title"); }
            set { SetProperty("$title", value); }
        }

        /// <summary>
        /// Gets or sets a collection of strings that are descriptive of the document.  These are primarily used for searching.
        /// </summary>
        public System.Collections.Generic.List<string> Tags
        {
            get
            {
                System.Collections.Generic.List<string> tags = new System.Collections.Generic.List<string>();
                string[] tagsAsStrings = (string[])GetProperty("$tags");
                for (int i = 0; i < tagsAsStrings.Length; i++)
                    tags.Add(tagsAsStrings[i]);
                return tags;
            }
            set { SetProperty("$tags", value); }
        }

        /// <summary>
        /// Gets or sets the user properties.
        /// </summary>
        /// <value>
        /// The user properties.
        /// </value>
        public System.Collections.Generic.Dictionary<string, object> UserProperties
        {
            get
            {
                if (_userProperties == null || _userProperties.Count <= 0)
                    PopulateUserPropertiesFromUnderlying();
                return _userProperties;
            }
            set
            {
                _userProperties = value;
            }
        }

        /// <summary>
        /// Populates the user properties from underlying <see cref="AssetBase"/> object's underling Dictionary.
        /// </summary>
        private void PopulateUserPropertiesFromUnderlying()
        {
            System.Collections.IEnumerator en = GetEnumerator();

            _userProperties = new System.Collections.Generic.Dictionary<string,object>();

            while (en.MoveNext())
            {
                Common.NetworkPackage.DictionaryEntry<string, object> entry =
                    (Common.NetworkPackage.DictionaryEntry<string, object>)en.Current;
                
                if (!MetaAsset.ReservedPropertyNames.Contains(entry.Key))
                    _userProperties.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Saves the user properties to underlying <see cref="AssetBase"/> object's underling Dictionary.
        /// </summary>
        private void SaveUserPropertiesToUnderlying()
        {
            System.Collections.Generic.Dictionary<string, object>.Enumerator en = _userProperties.GetEnumerator();

            while (en.MoveNext())
            {
                if (MetaAsset.ReservedPropertyNames.Contains(en.Current.Key))
                    throw new FieldAccessException("User properties contains a reserved property name and cannot.");

                // If the underlying dictionary has an entry for the current user property
                if (ContainsKey(en.Current.Key))
                {
                    // Then update
                    this[en.Current.Key] = en.Current.Value;
                }
                else
                {
                    // Then add
                    Add(en.Current.Key, en.Current.Value);
                }
            }
        }

        /// <summary>
        /// Instantiates a list of reserved properties (anything not existing here is consider a user property).
        /// </summary>
        /// <returns></returns>
        private static System.Collections.Generic.List<string> InstantiateReservedProperties()
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            list.Add("$lockedby");
            list.Add("$lockedat");
            list.Add("$creator");
            list.Add("$length");
            list.Add("$md5");
            list.Add("$extension");
            list.Add("$created");
            list.Add("$lastaccess");
            list.Add("$title");
            list.Add("$tags");
            return list;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is locked; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocked { get { return LockedAt != null && !string.IsNullOrEmpty(LockedBy); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaAsset"/> class.
        /// </summary>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        public MetaAsset(Database cdb)
            : base(cdb)
        {
            SetProperty("$tags", new System.Collections.Generic.List<string>());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaAsset"/> class.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        public MetaAsset(Guid guid, Database cdb)
            : base(guid, cdb)
        {
        }

        /// <summary>
        /// Creates an instance of a <see cref="MetaAsset"/> object.
        /// </summary>
        /// <param name="guid">A Guid that provides a unique reference to an Asset.</param>
        /// <param name="lockedby">The username of the user holding the outstanding lock.</param>
        /// <param name="lockedat">A timestamp of when the asset was locked, if null then not locked.</param>
        /// <param name="creator">The creator of this asset.</param>
        /// <param name="length">The quantity of bytes of the <see cref="DataAsset"/>.</param>
        /// <param name="md5">A checksum value of the <see cref="DataAsset"/>.</param>
        /// <param name="extension">The extension used by the <see cref="DataAsset"/> including the leading period (e.g., .exe, .txt, .docx, etc).</param>
        /// <param name="created">A timestamp marking the creation of this asset on the server.</param>
        /// <param name="lastaccess">A timestamp marking when this asset was last accessed on the server.</param>
        /// <param name="title">A descriptive title of this asset.</param>
        /// <param name="tags">A collection of strings that are descriptive of the document.  These are primarily used for searching.</param>
        /// <param name="userproperties">User defined propertes Key is the name or title of the property, object is the value of the property.</param>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public static MetaAsset Create(Guid guid, string lockedby, DateTime? lockedat, string creator,
            ulong length, string md5, string extension, DateTime created,
            DateTime lastaccess, string title, System.Collections.Generic.List<string> tags,
            System.Collections.Generic.Dictionary<string, object> userproperties, Database cdb)
        {
            MetaAsset ma = new MetaAsset(guid, cdb);
            ma.LockedBy = lockedby;
            ma.LockedAt = lockedat;
            ma.Creator = creator;
            ma.Length = length;
            ma.Md5 = md5;
            ma.Extension = extension;
            ma.Created = created;
            ma.LastAccess = lastaccess;
            ma.Title = title;

            for (int i = 0; i < tags.Count; i++)
                ma.Tags.Add(tags[i]);

            System.Collections.Generic.Dictionary<string, object>.Enumerator en = userproperties.GetEnumerator();
            while (en.MoveNext())
                ma.UserProperties.Add(en.Current.Key, en.Current.Value);

            return ma;
        }

        /// <summary>
        /// Copies this instance to a new <see cref="MetaAsset"/> using the specified new Guid, but does not
        /// make updates to the servers, thus, it is just a memory copy on the local system.
        /// </summary>
        /// <param name="newGuid">The new Guid.</param>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        /// <returns>The instantiated <see cref="MetaAsset"/> reference.</returns>
        public MetaAsset Copy(Guid newGuid, Database cdb)
        {
            MetaAsset newMa;

            newMa = MetaAsset.Create(newGuid, LockedBy, LockedAt, Creator, Length, Md5, Extension,
                Created, LastAccess, Title, Tags, UserProperties, cdb);

            return newMa;
        }

        /// <summary>
        /// Verifies that the underlying Dictionary has the required properties
        /// </summary>
        /// <returns>A error message or <c>null</c> if everything is proper.</returns>
        public string VerifyIntegrity()
        {
            if (!ContainsKey("$guid")) return "The required property '$guid' does not exist.";
            if (!ContainsKey("$creator")) return "The required property '$creator' does not exist.";
            if (!ContainsKey("$length")) return "The required property '$length' does not exist.";
            if (!ContainsKey("$md5")) return "The required property '$md5' does not exist.";
            if (!ContainsKey("$extension")) return "The required property '$extension' does not exist.";
            if (!ContainsKey("$created")) return "The required property '$created' does not exist.";
            if (!ContainsKey("$lastaccess")) return "The required property '$lastaccess' does not exist.";
            if (!ContainsKey("$title")) return "The required property '$title' does not exist.";
            if (!ContainsKey("$tags")) return "The required property '$tags' does not exist.";

            if (this["$guid"].GetType() != typeof(Guid)) return "The required property '$guid' must be of type string.";
            if (ContainsKey("$lockedby"))
            {
                if ((string)this["$lockedby"] != default(string) &&
                    this["$lockedby"].GetType() != typeof(string))
                    return "The property '$lockedby' must be of type string.";
            }
            if (ContainsKey("$lockedat"))
            {
                if (this["$lockedat"] != null &&
                    this["$lockedat"].GetType() != typeof(DateTime))
                    return "The property '$lockedat' must be of type DateTime.";
            }
            if (this["$creator"].GetType() != typeof(string)) return "The required property '$creator' must be of type string.";
            if (ContainsKey("$previousversion")) { if (this["$previousversion"].GetType() != typeof(string)) return "The property '$previousversion' must be of type string."; }
            if (ContainsKey("$nextversion")) { if (this["$nextversion"].GetType() != typeof(string)) return "The property '$nextversion' must be of type string."; }
            if (this["$length"].GetType() != typeof(ulong)) return "The required property '$length' must be of type ulong.";
            if (this["$md5"].GetType() != typeof(string)) return "The required property '$md5' must be of type string.";
            if (this["$extension"].GetType() != typeof(string)) return "The required property '$extension' must be of type string.";
            if (this["$created"].GetType() != typeof(DateTime)) return "The required property '$created' must be of type DateTime.";
            if (this["$lastaccess"].GetType() != typeof(DateTime)) return "The required property '$lastaccess' must be of type DateTime.";
            if (this["$title"].GetType() != typeof(string)) return "The required property '$title' must be of type string.";
            if (this["$tags"].GetType() != typeof(string[])) return "The required property '$tags' must be of type string[].";

            return null;
        }

        /// <summary>
        /// Loads this instance from the file specified.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool LoadFromLocal(Work.ResourceJobBase job, string relativeFilePath, FileSystem.IO fileSystem)
        {
            if (job != null && job.AbortAction) return false;

            // Read the file
            try
            {
                if (!ReadFromFile(relativeFilePath, fileSystem))
                {
                    Logger.General.Error("Failed to read the meta asset from " + relativeFilePath);
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to read the meta asset from " + relativeFilePath, e);
                return false;
            }

            // Verify valid meta asset
            if(!string.IsNullOrEmpty(VerifyIntegrity()))
            {
                Logger.General.Error("The file at relative path " + relativeFilePath + " is does not represent a valid meta asset.");
                return false;
            }

            _state.State = AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.LoadedFromLocal |
                AssetState.Flags.OnDisk;

            return true;
        }

        /// <summary>
        /// Saves this instance to a file specified.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool SaveToLocal(Work.ResourceJobBase job, string relativeFilePath, FileSystem.IO fileSystem)
        {
            if (job != null && job.AbortAction) return false;

            // Verify valid meta asset
            if (!string.IsNullOrEmpty(VerifyIntegrity()))
            {
                Logger.General.Error("The data in this meta asset instance is invalid for a meta asset.");
                return false;
            }

            // Save the file
            try
            {
                SaveToFile(relativeFilePath, fileSystem, true);
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to write the meta asset to " + relativeFilePath, e);
                return false;
            }

            _state.State |= AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.OnDisk;

            return true;
        }

        public bool GetFromRemote(Work.ResourceJobBase job, out string errorMessage)
        {
            Result result;

            errorMessage = null;

            if (job != null && job.AbortAction) return false;
            
            _doc = new Document(GuidString);

            result = _doc.DownloadSync(_database.Server, _database, false);

            if (!result.IsPass)
            {
                Logger.General.Error("Failed to download the couchdb resource with id " + GuidString);
                errorMessage = "Failed to download the meta asset.";
                return false;
            }

            return true;
        }
    }
}
