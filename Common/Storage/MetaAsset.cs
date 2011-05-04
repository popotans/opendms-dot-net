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
        /// <summary>
        /// Represents the method that handles an event.
        /// </summary>
        /// <param name="sender">The <see cref="DataAsset"/> that triggered the event.</param>
        public delegate void EventHandler(MetaAsset sender);
        /// <summary>
        /// Represents the method that handles a progress event.
        /// </summary>
        /// <param name="sender">The <see cref="DataAsset"/> that triggered the event.</param>
        /// <param name="packetSize">Size of the packet.</param>
        /// <param name="headersTotal">The headers total.</param>
        /// <param name="contentTotal">The content total.</param>
        /// <param name="total">The total.</param>
        public delegate void ProgressHandler(MetaAsset sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);

        /// <summary>
        /// Fired to indicate a timeout
        /// </summary>
        public event EventHandler OnTimeout;

        /// <summary>
        /// Fired to indicate progress of an upload
        /// </summary>
        public event ProgressHandler OnUploadProgress;

        /// <summary>
        /// Fired to indicate progress of a download
        /// </summary>
        public event ProgressHandler OnDownloadProgress;

        private static System.Collections.Generic.List<string> ReservedPropertyNames = InstantiateReservedProperties();

        private Document _doc;
        private System.Collections.Generic.Dictionary<string, object> _userProperties;

        public Document Document 
        {
            get { return _doc; }
            set { _doc = value; }
        }

        public string RelativePath
        {
            get { return FileSystem.Path.RelativeMetaPath + GuidString + ".xml"; }
        }

        public string FullPath
        {
            get { return FileSystem.Path.FullMetaPath + GuidString + ".xml"; }
        }

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
            get { return (DateTime)GetProperty("$lastaccess"); }
            set { SetProperty("$lastaccess", value); }
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
                object o = GetProperty("$tags");
                System.Collections.Generic.List<string> tags = new System.Collections.Generic.List<string>();

                if (o.GetType() == typeof(string[]))
                {
                    string[] tagsAsStrings = (string[])o;
                    for (int i = 0; i < tagsAsStrings.Length; i++)
                        tags.Add(tagsAsStrings[i]);

                    return tags;
                }
                else if (o.GetType() == typeof(System.Collections.Generic.List<string>))
                {
                    return (System.Collections.Generic.List<string>)o;
                }
                else
                    throw new InvalidCastException("Unsupported data type.");
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
            list.Add("$guid");
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

        public MetaAsset()
        {
            SetProperty("$tags", new System.Collections.Generic.List<string>());
        }

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
            SetProperty("$tags", new System.Collections.Generic.List<string>());
        }

        /// <summary>
        /// Instantiates an instance of a <see cref="MetaAsset"/> object.
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
        public static MetaAsset Instantiate(Guid guid, string lockedby, DateTime? lockedat, string creator,
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

            newMa = MetaAsset.Instantiate(newGuid, LockedBy, LockedAt, Creator, Length, Md5, Extension,
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
            if (this["$length"].GetType() != typeof(ulong)) return "The required property '$length' must be of type ulong.";
            if (this["$md5"].GetType() != typeof(string)) return "The required property '$md5' must be of type string.";
            if (this["$extension"].GetType() != typeof(string)) return "The required property '$extension' must be of type string.";
            if (this["$created"].GetType() != typeof(DateTime)) return "The required property '$created' must be of type DateTime.";
            if (this["$lastaccess"].GetType() != typeof(DateTime)) return "The required property '$lastaccess' must be of type DateTime.";
            if (this["$title"].GetType() != typeof(string)) return "The required property '$title' must be of type string.";
            if (this["$tags"].GetType() != typeof(string[]) && this["$tags"].GetType() != typeof(System.Collections.Generic.List<string>)) 
                return "The required property '$tags' must be of type string[].";

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
            string error;

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
            if(!string.IsNullOrEmpty(error = VerifyIntegrity()))
            {
                Logger.General.Error("The file at relative path " + relativeFilePath + " is does not represent a valid meta asset.  Integrity checking returned: " + error);
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
        /// <param name="fileSystem">The file system.</param>
        /// <returns>
        ///   <c>True</c> if successful; otherwise, <c>false</c>.
        /// </returns>
        public bool SaveToLocal(Work.ResourceJobBase job, FileSystem.IO fileSystem)
        {
            string error;

            if (job != null && job.AbortAction) return false;

            // Verify valid meta asset
            if (!string.IsNullOrEmpty(error = VerifyIntegrity()))
            {
                Logger.General.Error("The data in this meta asset instance is invalid for a meta asset.  Integrity checking returned: " + error);
                return false;
            }

            // Save the file
            try
            {
                SaveToFile(RelativePath, fileSystem, true);
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to write the meta asset to " + FullPath, e);
                return false;
            }

            _state.State |= AssetState.Flags.CanTransfer |
                AssetState.Flags.InMemory |
                AssetState.Flags.OnDisk;

            return true;
        }

        /// <summary>
        /// Gets from remote.
        /// </summary>
        /// <param name="job">The <see cref="Work.ResourceJobBase"/>.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool GetFromRemote(Work.ResourceJobBase job, int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            Result result;

            errorMessage = null;

            if (job != null && job.AbortAction) return false;

            if (Database == null) 
                throw new InvalidOperationException("Property Database cannot be null.");
            
            _doc = new Document(GuidString);

            Logger.General.Debug("Starting download of resource " + GuidString + " for job id " + job.Id.ToString());

            _doc.OnDownloadProgress += new CouchDB.Document.ProgressEventHandler(Document_OnDownloadProgress);
            _doc.OnUploadProgress += new CouchDB.Document.ProgressEventHandler(Document_OnUploadProgress);
            _doc.OnTimeout += new CouchDB.Document.EventHandler(Document_OnTimeout);
            result = _doc.Download(Database, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize);

            Logger.General.Debug("Finished download of resource " + GuidString + " for job id " + job.Id.ToString());

            if (!result.IsPass)
            {
                Logger.General.Error("Failed to download the couchdb resource with id " + GuidString);
                errorMessage = "Failed to download the meta asset.";
                return false;
            }

            if (!string.IsNullOrEmpty(errorMessage = ImportFromDocument(_doc)))
            {
                Logger.General.Error("Failed to import the couchdb resource with id " + GuidString + 
                    ", the error message was '" + errorMessage + "'.");
                errorMessage = "Failed to import the meta asset.";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Creates this MetaAsset on the remote host.
        /// </summary>
        /// <param name="job">The <see cref="Work.ResourceJobBase"/>.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>True</c> if successful; otherwise, <c>False</c>.
        /// </returns>
        public bool CreateOnRemote(Work.ResourceJobBase job, int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            Result result;

            _doc = new Document(GuidString);
            errorMessage = null;

            if (job != null && job.AbortAction) return false;

            if (Database == null)
                throw new InvalidOperationException("Property Database cannot be null.");
            
            if (!string.IsNullOrEmpty(errorMessage = ExportToDocument()))
                return false;

            if (!_doc.CanCreate)
                throw new InvalidOperationException("The underlying Document's state will not allow creation.");

            _doc.OnDownloadProgress += new CouchDB.Document.ProgressEventHandler(Document_OnDownloadProgress);
            _doc.OnUploadProgress += new CouchDB.Document.ProgressEventHandler(Document_OnUploadProgress);
            _doc.OnTimeout += new CouchDB.Document.EventHandler(Document_OnTimeout);
            result = _doc.Create(Database, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize);

            if (!result.IsPass)
            {
                errorMessage = result.Message;
                return false;
            }

            return true;
        }

        public bool UpdateOnRemote(Work.ResourceJobBase job, int sendBufferSize, int receiveBufferSize, out string errorMessage)
        {
            CouchDB.Result result;
            errorMessage = null;

            if (job != null && job.AbortAction) return false;

            if (Database == null)
                throw new InvalidOperationException("Property Database cannot be null.");

            if (!string.IsNullOrEmpty(errorMessage = ExportToDocument()))
                return false;

            if (!_doc.CanUpdate)
                throw new InvalidOperationException("The underlying Document's state will not allow updating.");

            _doc.OnDownloadProgress += new CouchDB.Document.ProgressEventHandler(Document_OnDownloadProgress);
            _doc.OnUploadProgress += new CouchDB.Document.ProgressEventHandler(Document_OnUploadProgress);
            _doc.OnTimeout += new CouchDB.Document.EventHandler(Document_OnTimeout);
            result = _doc.Create(Database, (int)job.Timeout, (int)job.Timeout, sendBufferSize, receiveBufferSize);

            if (!result.IsPass)
            {
                errorMessage = result.Message;
                return false;
            }

            return true;
        }

        private string ImportFromDocument(Document doc)
        {
            // $guid is in doc.Id
            if (!doc.Contains("$creator")) return "The required property '$creator' does not exist.";
            if (!doc.Contains("$length")) return "The required property '$length' does not exist.";
            if (!doc.Contains("$md5")) return "The required property '$md5' does not exist.";
            if (!doc.Contains("$extension")) return "The required property '$extension' does not exist.";
            if (!doc.Contains("$created")) return "The required property '$created' does not exist.";
            if (!doc.Contains("$lastaccess")) return "The required property '$lastaccess' does not exist.";
            if (!doc.Contains("$title")) return "The required property '$title' does not exist.";
            if (!doc.Contains("$tags")) return "The required property '$tags' does not exist.";

            if (doc.Contains("$lockedby"))
            {
                if ((string)doc["$lockedby"] != default(string) &&
                    doc["$lockedby"].GetType() != typeof(string))
                    return "The property '$lockedby' must be of type string.";
            }
            if (doc.Contains("$lockedat"))
            {
                if (doc["$lockedat"] != null &&
                    doc["$lockedat"].GetType() != typeof(string))
                    return "The property '$lockedat' must be of type string.";
            }


            if (ContainsKey("$guid")) this["$guid"] = new Guid(doc.Id);
            else Add("$guid", new Guid(doc.Id));

            if (ContainsKey("$creator")) this["$creator"] = doc.GetPropertyAsString("$creator");
            else Add("$creator", doc.GetPropertyAsString("$creator"));
            
            if (ContainsKey("$length")) this["$length"] = doc.GetPropertyAsUInt64("$length");
            else Add("$length", doc.GetPropertyAsUInt64("$length"));

            if (ContainsKey("$md5")) this["$md5"] = doc.GetPropertyAsString("$md5");
            else Add("$md5", doc.GetPropertyAsString("$md5"));

            if (ContainsKey("$extension")) this["$extension"] = doc.GetPropertyAsString("$extension");
            else Add("$extension", doc.GetPropertyAsString("$extension"));

            if (ContainsKey("$created")) this["$created"] = doc.GetPropertyAsDateTime("$created");
            else Add("$created", doc.GetPropertyAsDateTime("$created"));

            if (ContainsKey("$lastaccess")) this["$lastaccess"] = doc.GetPropertyAsDateTime("$lastaccess");
            else Add("$lastaccess", doc.GetPropertyAsDateTime("$lastaccess"));

            if (ContainsKey("$title")) this["$title"] = doc.GetPropertyAsString("$title");
            else Add("$title", doc.GetPropertyAsString("$title"));

            if (ContainsKey("$tags")) this["$tags"] = doc.GetPropertyAsList<string>("$tags").ToArray();
            else Add("$tags", doc.GetPropertyAsList<string>("$tags").ToArray());

            return null;
        }

        private string ExportToDocument()
        {
            string str;

            if (!string.IsNullOrEmpty(str = VerifyIntegrity()))
                return str;

            if (_doc == null)
                throw new InvalidOperationException("CouchDB Document is null.");

            _doc.SetProperty("$creator", Creator);
            _doc.SetProperty("$length", Length);
            _doc.SetProperty("$md5", Md5);
            _doc.SetProperty("$extension", Extension);
            _doc.SetProperty("$created", Created);
            _doc.SetProperty("$lastaccess", LastAccess);
            _doc.SetProperty("$title", Title);
            _doc.SetProperty("$tags", Tags);

            return null;
        }

        public static MetaAsset LoadFromLocal(string relativeFilePath, Database cdb, FileSystem.IO fileSystem)
        {
            MetaAsset ma = new MetaAsset(cdb);

            if (!ma.LoadFromLocal(null, relativeFilePath, fileSystem))
                return null;

            return ma;
        }

        void Document_OnTimeout(Document sender, Http.Client httpClient)
        {
            if (OnTimeout != null) OnTimeout(this);
        }

        void Document_OnUploadProgress(Document sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnUploadProgress != null)
                OnUploadProgress(this, packetSize, headersTotal, contentTotal, total);
        }

        void Document_OnDownloadProgress(Document sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnDownloadProgress != null)
                OnDownloadProgress(this, packetSize, headersTotal, contentTotal, total);
        }
    }
}
