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

        private string _extension = null;
        public string Extension { get { return _extension; } }

        /// <summary>
        /// The quantity of bytes completed.
        /// </summary>
        public ulong BytesComplete;
        /// <summary>
        /// The total quantity of bytes.
        /// </summary>
        public ulong BytesTotal;

        public string RelativePath
        {
            get 
            {
                if (string.IsNullOrEmpty(_extension))
                    throw new InvalidOperationException("An extension has not been set.");

                return FileSystem.Path.RelativeDataPath + GuidString + _extension;
            }
        }

        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(_extension))
                    throw new InvalidOperationException("An extension has not been set.");

                return FileSystem.Path.FullDataPath + GuidString + _extension; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="ma">The <see cref="MetaAsset"/> that is paired with this <see cref="DataAsset"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        public DataAsset(MetaAsset ma, Database cdb)
            : this(ma.Guid, ma.Extension, cdb)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAsset"/> class.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> providing a unique reference to the Asset.</param>
        /// <param name="extension">The extension of the resource (e.g., .doc, .xsl, .odt)</param>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        public DataAsset(Guid guid, string extension, Database cdb)
            : base(guid, cdb)
        {
            BytesComplete = BytesTotal = 0;
            _extension = extension;
            _state = new AssetState() { State = AssetState.Flags.CanTransfer };
        }

        public System.IO.Stream GetDownloadStream(Work.ResourceJobBase job, Document doc, out string errorMessage)
        {
            errorMessage = null;

            if (job != null && job.AbortAction) return null;

            // Attachment checking
            if (doc.Attachments.Count <= 0)
            {
                errorMessage = "No Attachment was found in the Document.";
                return null;
            }
            if (doc.Attachments.Count > 1)
            {
                errorMessage = "Only a one Attachment can be present in a Document.";
                return null;
            }

            // Ensure it can be downloaded
            if (!doc.Attachments[0].CanDownload)
            {
                errorMessage = "The Attachment cannot be downloaded.";
                return null;
            }

            return doc.Attachments[0].GetDownloadStreamSync(Database.Server, Database,
                Web.DataStreamMethod.Stream, false, false, true);
        }

        public bool DownloadAndSaveLocally(Work.ResourceJobBase job, MetaAsset ma,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            if (ma.Document == null)
                ma.Document = new Document(ma.GuidString);

            return DownloadAndSaveLocally(job, ma.Document, 
                fileSystem, out errorMessage);
        }

        public bool DownloadAndSaveLocally(Work.ResourceJobBase job, Document doc,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            FileSystem.IOStream iostream;
            System.IO.Stream stream;
            errorMessage = null;

            if (job != null && job.AbortAction) return false;

            // Get the download stream
            if ((stream = GetDownloadStream(job, doc, out errorMessage)) == null)
                return false;

            // Open the local stream
            try
            {
                iostream = fileSystem.Open(RelativePath, System.IO.FileMode.Create,
                    System.IO.FileAccess.Write, System.IO.FileShare.None, System.IO.FileOptions.None,
                    "Common.Storage.DataAsset.DownloadAndSaveLocally()");
            }
            catch(Exception e)
            {
                errorMessage = "Failed to open an iostream for " + RelativePath;
                Logger.General.Error("Failed to open an iostream for " + FullPath + " resulting in the following exception.", e);
                return false;
            }

            // Copy the download stream into the local stream
            try
            {
                iostream.CopyFrom(stream);
            }
            catch (Exception e)
            {
                errorMessage = "Failed to copy the data received to the file " + RelativePath;
                Logger.General.Error("Failed to copy the data received on the network stream to the file " + FullPath + " resulting in the following exception.", e);
                return false;
            }

            // Close the network stream
            stream.Close();
            stream.Dispose();

            // Close the filestream
            fileSystem.Close(iostream);

            return true;
        }

        public bool CreateOnRemote(Work.ResourceJobBase job, MetaAsset ma,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            if (ma.Document == null)
            {
                errorMessage = "Invalid CouchDB Document.";
                return false;
            }

            return CreateOnRemote(job, ma.Document,
                fileSystem, out errorMessage);
        }

        public bool CreateOnRemote(Work.ResourceJobBase job, Document doc,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            CouchDB.Result result;
            FileSystem.IOStream iostream;
            string filename;
            Attachment att;
            errorMessage = null;

            if (job != null && job.AbortAction) return false;

            filename = System.IO.Path.GetFileName(RelativePath);

            if(doc.GetPropertyAsUInt64("$length") > long.MaxValue)
                throw new OverflowException("The length is to large to cast to long.");

            // Create Attachment
            att = new Attachment(filename, CouchDB.Utilities.MimeType(filename), 
                (long)doc.GetPropertyAsUInt64("$length"), doc);

            // Supply a stream
            try
            {
                iostream = fileSystem.Open(RelativePath, System.IO.FileMode.Open, 
                    System.IO.FileAccess.Read, System.IO.FileShare.None, 
                    System.IO.FileOptions.None, 
                    "Common.Storage.DataAsset.CreateOnRemote()");
            }
            catch(Exception e)
            {
                Logger.General.Error("Could not open the stream for " + FullPath, e);
                errorMessage = "Failed to open file " + RelativePath;
                return false;
            }

            try
            {
                att.Stream = iostream.Stream;
            }
            catch (Exception e)
            {
                string a = e.Message;
            }

            doc.Attachments.Add(att);

            result = doc.Attachments[0].UploadSync(Database.Server, Database, 
                Web.DataStreamMethod.Stream, false, true);

            if (!result.IsPass)
            {
                errorMessage = result.Message;
                Logger.General.Error("Uploading of attachment failed with message: " +
                    result.Message);
                return false;
            }

            return true;
        }

        public bool UpdateOnRemote(Work.ResourceJobBase job, MetaAsset ma,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            if (ma.Document == null)
                ma.Document = new Document(ma.GuidString);

            return UpdateOnRemote(job, ma.Document,
                fileSystem, out errorMessage);
        }

        public bool UpdateOnRemote(Work.ResourceJobBase job, Document doc,
            FileSystem.IO fileSystem, out string errorMessage)
        {
            // The way our code works, on an update the MetaAsset actually issues an update
            // which creates a new version on the couchdb server and this must be done
            // using an update command.  However, an attachment will use the same
            // command as the create because it is just simply added to the current
            // CouchDB version.  Essentially, we can just use the CreateOnRemote method!
            // This we piggy-back it.
            return CreateOnRemote(job, doc, fileSystem, out errorMessage);
        }
    }
}