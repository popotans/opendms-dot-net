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

namespace Common.Data
{
    /// <summary>
    /// Represents a pair of <see cref="MetaAsset"/> and <see cref="DataAsset"/>
    /// </summary>
    public class FullAsset
    {
        /// <summary>
        /// Gets or sets the <see cref="MetaAsset"/>.
        /// </summary>
        /// <value>
        /// The <see cref="MetaAsset"/>.
        /// </value>
        public MetaAsset MetaAsset { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DataAsset"/>.
        /// </summary>
        /// <value>
        /// The <see cref="DataAsset"/>.
        /// </value>
        public DataAsset DataAsset { get; set; }

        /// <summary>
        /// Gets the unique identifier which is common to both the <see cref="MetaAsset"/> and the <see cref="DataAsset"/>.
        /// </summary>
        public Guid Guid
        {
            get
            {
                if (MetaAsset != null) return MetaAsset.Guid;
                else if (DataAsset != null) return DataAsset.Guid;
                else throw new InvalidAssetStateException(null, "Both MetaAsset and DataAsset cannot be null.");
            }
        }

        /// <summary>
        /// This is a crappy implementation to provide support for CreateResourceJob to allow it to set the Guid 
        /// assigned on the client.  Ideally in the future this will be removed.
        /// </summary>
        public Guid PreviousGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullAsset"/> class.
        /// </summary>
        /// <param name="ma">The <see cref="MetaAsset"/>.</param>
        /// <param name="da">The <see cref="DataAsset"/>.</param>
        public FullAsset(MetaAsset ma, DataAsset da)
        {
            MetaAsset = ma;
            DataAsset = da;
        }

        /// <summary>
        /// Gets the header information for this asset from the server including an <see cref="ETag"/> and a string value
        /// representing the MD5 of the data asset, by calling <see cref="M:MetaAsset.GetHeadFromServer"/>.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.ResourceJobBase"/> calling this method.</param>
        /// <returns>The <see cref="ETag"/> from the server.</returns>
        public Head GetHeadFromServer(Work.ResourceJobBase job)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download");

            return MetaAsset.GetHeadFromServer(job);
        }

        /// <summary>
        /// Gets the <see cref="ETag"/> for this asset from the server by calling <see cref="M:MetaAsset.GetETagFromServer"/>.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.ResourceJobBase"/> calling this method.</param>
        /// <returns>The <see cref="ETag"/> from the server.</returns>
        public ETag GetETagFromServer(Work.ResourceJobBase job)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download");

            return MetaAsset.GetETagFromServer(job);
        }

        /// <summary>
        /// Downloads both the <see cref="MetaAsset"/> and the <see cref="DataAsset"/> from the server.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.ResourceJobBase"/> calling this method.</param>
        /// <returns><c>True</c> if successful; otherwise <c>false</c>.</returns>
        public bool DownloadFromServer(Work.ResourceJobBase job)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");
            if (DataAsset == null)
                throw new Work.JobException("DataAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download MetaAsset");

            if (!DataAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download DataAsset");

            if (!MetaAsset.DownloadFromServer(job))
            {
                job.SetErrorFlag();
                return false;
            }

            job.UpdateLastAction();

            if (!DataAsset.DownloadFromServer(job, MetaAsset))
            {
                job.SetErrorFlag();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates both the <see cref="MetaAsset"/> and the <see cref="DataAsset"/> on the server.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.ResourceJobBase"/> calling this method.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        /// <returns>
        ///   <c>True</c> if successful; otherwise <c>false</c>.
        /// </returns>
        public bool CreateOnServer(Work.ResourceJobBase job, FileSystem.IO fileSystem)
        {
            Guid newGuid;
            Data.MetaAsset oldMa = MetaAsset;

            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");
            if (DataAsset == null)
                throw new Work.JobException("DataAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot upload MetaAsset");

            if (!DataAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot upload DataAsset");

            NetworkPackage.ServerResponse sr;

            PreviousGuid = MetaAsset.Guid;

            sr = MetaAsset.CreateOnServer(job);
            if (!(bool)sr["Pass"])
            {
                job.SetErrorFlag();
                return false;
            }

            job.UpdateLastAction();
            
            // Get the newGuid from the response
            newGuid = new Guid((string)sr["Message"]);

            // Update the MetaAsset to change to the new Guid and save it to the file system
            MetaAsset = MetaAsset.CopyToNew(newGuid, true, fileSystem);

            // Delete the old MA
            oldMa.Resource.DeleteFromFilesystem();

            // Rename the DA
            DataAsset.RenameTo(newGuid);

            sr = DataAsset.SaveToServer(job, MetaAsset);
            if (!(bool)sr["Pass"])
            {
                job.SetErrorFlag();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves both the <see cref="MetaAsset"/> and the <see cref="DataAsset"/> to the server.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.ResourceJobBase"/> calling this method.</param>
        /// <returns><c>True</c> if successful; otherwise <c>false</c>.</returns>
        public bool SaveToServer(Work.ResourceJobBase job)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");
            if (DataAsset == null)
                throw new Work.JobException("DataAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot upload MetaAsset");

            if (!DataAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot upload DataAsset");

            NetworkPackage.ServerResponse sr;

            sr = MetaAsset.SaveToServer(job);
            if (!(bool)sr["Pass"])
            {                
                job.SetErrorFlag();
                return false;
            }

            job.UpdateLastAction();

            sr = DataAsset.SaveToServer(job, MetaAsset);
            if (!(bool)sr["Pass"])
            {
                job.SetErrorFlag();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the specified job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        public bool Load(Work.ResourceJobBase job)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");
            if (DataAsset == null)
                throw new Work.JobException("DataAsset cannot be null.");

            return MetaAsset.Load(job);
        }
    }
}