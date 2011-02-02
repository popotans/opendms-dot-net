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
        /// Gets the <see cref="ETag"/> for this asset from the server by calling <see cref="M:MetaAsset.GetETagFromServer"/>.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> logger where network events are documented.</param>
        /// <returns>The <see cref="ETag"/> from the server.</returns>
        public ETag GetETagFromServer(Work.AssetJobBase job, Logger networkLogger)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download");

            return MetaAsset.GetETagFromServer(job, networkLogger);
        }

        /// <summary>
        /// Downloads both the <see cref="MetaAsset"/> and the <see cref="DataAsset"/> from the server.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> logger where network events are documented.</param>
        /// <returns><c>True</c> if successful; otherwise <c>false</c>.</returns>
        public bool DownloadFromServer(Work.AssetJobBase job, Logger networkLogger)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");
            if (DataAsset == null)
                throw new Work.JobException("DataAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download MetaAsset");

            if (!DataAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download DataAsset");

            if (!MetaAsset.DownloadFromServer(job, networkLogger))
            {
                job.SetErrorFlag();
                return false;
            }

            job.UpdateLastAction();

            if (!DataAsset.DownloadFromServer(job, MetaAsset, networkLogger))
            {
                job.SetErrorFlag();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves both the <see cref="MetaAsset"/> and the <see cref="DataAsset"/> to the server.
        /// </summary>
        /// <param name="job">A reference to the <see cref="Work.AssetJobBase"/> calling this method.</param>
        /// <param name="networkLogger">A reference to the <see cref="Logger"/> logger where network events are documented.</param>
        /// <returns><c>True</c> if successful; otherwise <c>false</c>.</returns>
        public bool SaveToServer(Work.AssetJobBase job, Logger networkLogger)
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

            sr = MetaAsset.SaveToServer(job, networkLogger);
            if (!(bool)sr["Pass"])
            {                
                job.SetErrorFlag();
                return false;
            }

            job.UpdateLastAction();

            sr = DataAsset.SaveToServer(job, MetaAsset, networkLogger);
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
        public bool Load(Work.AssetJobBase job)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");
            if (DataAsset == null)
                throw new Work.JobException("DataAsset cannot be null.");

            return MetaAsset.Load(job);
        }
    }
}