using System;

namespace Common.Work
{
    public abstract class AssetJobBase : JobBase
    {
        protected Data.FullAsset _fullAsset;
        public Data.FullAsset FullAsset { get { return _fullAsset; } }

        public AssetJobBase(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ProgressMethodType progressMethod, 
            ErrorManager errorManager, FileSystem.IO fileSystem,
            Logger generalLogger, Logger networkLogger)
            : base(requestor, id, actUpdateUI, timeout, progressMethod, errorManager, 
            fileSystem, generalLogger, networkLogger)
        {
            _fullAsset = fullAsset;
        }
    }
}
