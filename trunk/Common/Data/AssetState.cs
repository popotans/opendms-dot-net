using System;

namespace Common.Data
{
    public class AssetState
    {
        public enum Flags
        {
            None = 0,
            LoadedFromLocal = 1,
            LoadedFromRemote = 2,
            InMemory = 4,
            MemoryDirty = 8,
            OnDisk = 16,
            DiskDirty = 32,
            CanTransfer
        }

        public Flags State { get; set; }

        public AssetState()
        {
            this.State = AssetState.Flags.None;
        }

        public AssetState(Flags flags)
        {
            this.State = flags;
        }

        public bool HasFlag(Flags flags)
        {
            return (State & flags) == flags;
        }
    }
}
