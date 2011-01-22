using System;
using System.Collections.Generic;

namespace WindowsClient
{
    public class TVIState
    {
        public enum EventFlagType
        {
            None = 0,
            Loading = 1,
            Loaded = 2,
            Canceled = 4,
            Timeout = 8,
            Error = 16
        }

        public enum ResourceStatusType
        {
            None = 0,
            LocalIsNewer = 1,
            LocalIsOlder = 2,
            LocalMatchesRemote = 4,
            RemoteExists = 8,
            RemoteExistsIsKnown = 16,
            LocalExists = 32,
            LocalExistsIsKnown = 64
        }

        private EventFlagType _eventFlags;
        private ResourceStatusType _resourceStatusFlags;

        public Common.Data.FullAsset FullAsset { get; set; }

        public bool IsLoading { get { return HasFlags(EventFlagType.Loading); } }
        public bool IsLoaded { get { return HasFlags(EventFlagType.Loaded); } }
        public bool IsCanceled { get { return HasFlags(EventFlagType.Canceled); } }
        public bool IsTimeout { get { return HasFlags(EventFlagType.Timeout); } }
        public bool IsError { get { return HasFlags(EventFlagType.Error); } }

        public bool IsLocalNewer { get { return HasFlags(ResourceStatusType.LocalIsNewer); } }
        public bool IsLocalOlder { get { return HasFlags(ResourceStatusType.LocalIsOlder); } }
        public bool IsLocalSameAsRemote { get { return HasFlags(ResourceStatusType.LocalMatchesRemote); } }
        public bool IsRemoteExistant { get { return HasFlags(ResourceStatusType.RemoteExists); } }
        public bool IsRemoteExistantKnown { get { return HasFlags(ResourceStatusType.RemoteExistsIsKnown); } }
        public bool IsLocalExistant { get { return HasFlags(ResourceStatusType.LocalExists); } }
        public bool IsLocalExistantKnown { get { return HasFlags(ResourceStatusType.LocalExistsIsKnown); } }

        public bool CanSave { get { return IsLocalExistantKnown && IsLocalExistant && IsLocalNewer; } }
        public bool CanGet { get { return IsRemoteExistantKnown && IsRemoteExistant && (IsLocalOlder || IsLocalSameAsRemote); } }
        

        public TVIState(Common.Data.FullAsset fullAsset)
        {
            _eventFlags = EventFlagType.None;
            _resourceStatusFlags = ResourceStatusType.None;
            FullAsset = fullAsset;
        }

        private bool HasFlags(EventFlagType flag)
        {
            return (_eventFlags & flag) == flag;
        }

        private bool HasFlags(ResourceStatusType flag)
        {
            return (_resourceStatusFlags & flag) == flag;
        }

        public EventFlagType GetEventFlags()
        {
            return _eventFlags;
        }

        public ResourceStatusType GetResourceStatusFlags()
        {
            return _resourceStatusFlags;
        }

        public void SetFlags(EventFlagType flags)
        {
            _eventFlags = flags;
        }

        public void SetFlags(ResourceStatusType flags)
        {
            _resourceStatusFlags = flags;
        }

        public void ClearFlags(ResourceStatusType flags)
        {
            _resourceStatusFlags &= ~flags;
        }

        public void UpdateEvent(bool isLoading, bool isLoaded, bool isCanceled,
            bool isTimeout, bool isError)
        {
            if (isLoaded) _eventFlags |= EventFlagType.Loaded;
            else _eventFlags &= ~EventFlagType.Loaded;

            if (isLoading) _eventFlags |= EventFlagType.Loading;
            else _eventFlags &= ~EventFlagType.Loading;

            if (isCanceled) _eventFlags |= EventFlagType.Canceled;
            else _eventFlags &= ~EventFlagType.Canceled;

            if (isTimeout) _eventFlags |= EventFlagType.Timeout;
            else _eventFlags &= ~EventFlagType.Timeout;

            if (isError) _eventFlags |= EventFlagType.Error;
            else _eventFlags &= ~EventFlagType.Error;
        }

        public void UpdateResourceStatus(bool? localIsNewer, bool? localIsOlder, 
            bool? localMatchesRemote, bool? remoteExists, bool? localExists)
        {
            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags, 
                ResourceStatusType.LocalIsNewer, localIsNewer);

            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.LocalIsOlder, localIsOlder);

            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.LocalMatchesRemote, localMatchesRemote);

            if (remoteExists.HasValue) _resourceStatusFlags |= ResourceStatusType.RemoteExistsIsKnown;
            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.RemoteExists, remoteExists);

            if (localExists.HasValue) _resourceStatusFlags |= ResourceStatusType.LocalExistsIsKnown;
            _resourceStatusFlags = SetResourceStatusFlag(_resourceStatusFlags,
                ResourceStatusType.LocalExists, localExists);
        }

        private ResourceStatusType SetResourceStatusFlag(
            ResourceStatusType currentFlags,
            ResourceStatusType flagToToggle,
            bool? newValue)
        {
            if(!newValue.HasValue) return currentFlags;

            if (newValue.Value) return currentFlags | flagToToggle;
            else return currentFlags & ~flagToToggle;
        }
    }
}
