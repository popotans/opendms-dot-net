using System;

namespace Common.Data
{
    public class FullAsset
    {
        public MetaAsset MetaAsset { get; set; }
        public DataAsset DataAsset { get; set; }
        public Guid Guid
        {
            get
            {
                if (MetaAsset != null) return MetaAsset.Guid;
                else if (DataAsset != null) return DataAsset.Guid;
                else throw new InvalidAssetStateException(null, "Both MetaAsset and DataAsset cannot be null.");
            }
        }

        public FullAsset(MetaAsset ma, DataAsset da)
        {
            MetaAsset = ma;
            DataAsset = da;
        }

        public ETag GetETagFromServer(Work.AssetJobBase job, Logger networkLogger)
        {
            if (MetaAsset == null)
                throw new Work.JobException("MetaAsset cannot be null.");

            if (!MetaAsset.AssetState.HasFlag(AssetState.Flags.CanTransfer))
                throw new InvalidAssetStateException(MetaAsset.AssetState, "Cannot download");

            return MetaAsset.GetETagFromServer(job, networkLogger);
        }

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