using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Common.Storage;

namespace WindowsClient
{
    /// <summary>
    /// Interaction logic for ResourceTreeView.xaml
    /// </summary>
    public partial class ResourceTreeView : UserControl
    {
        /// <summary>
        /// Represents the state of a TreeViewItem in the main window.
        /// </summary>
        public class State
        {
            public enum EventFlagType
            {
                None = 0,

                // Directional Event Flags

                Creating = 1,
                Updating = 2,
                Releasing = 4,
                Downloading = 8,

                // Non-Directional Event Flags

                Loaded = 1024,
                Canceled = 2048,
                Timeout = 4096,
                Error = 8192
            }

            public enum StatusType
            {
                /// <summary>
                /// None set.
                /// </summary>
                None = 0,
                /// <summary>
                /// The value contained in LocalExists is accurate.
                /// </summary>
                LocalExistsIsKnown = 1,
                /// <summary>
                /// The vaule contained in RemoteExists is accurate.
                /// </summary>
                RemoteExistsIsKnown = 2,
                /// <summary>
                /// The local asset/resource exists.
                /// </summary>
                /// <remarks>Beware that this data is not accurate unless LocalExistsIsKnown is <c>true</c>.</remarks>
                LocalExists = 4,
                /// <summary>
                /// The remote asset/resource exists.
                /// </summary>
                /// <remarks>Beware that this data is not accurate unless RemoteExistsIsKnown is <c>true</c>.</remarks>
                RemoteExists = 8,
                /// <summary>
                /// The local asset/resource is newer than the remote asset/resource.
                /// </summary>
                LocalIsNewer = 16,
                /// <summary>
                /// The local asset/resource is older than the remote asset/resource.
                /// </summary>
                LocalIsOlder = 32,
                /// <summary>
                /// The local asset/resource is the same age (version) as the remote.
                /// </summary>
                LocalMatchesRemote = 64
            }

            public enum DirectionType
            {
                None = 0,
                LocalToRemote = 1,
                RemoteToLocal = 2
            }

            public Common.Work.Master.JobType JobType { get; set; }                

            private EventFlagType _eventFlags;
            private StatusType _status;
            private Common.Storage.Version _resource;
            private DirectionType _direction;

            public Common.Storage.Version Resource { get { return _resource; } set { _resource = value; } }


            /// <summary>
            /// Gets a value indicating whether this instance is being created on the remote server.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is being created on the remote server; otherwise, <c>false</c>.
            /// </value>
            public bool IsCreating { get { return HasFlags(EventFlagType.Creating); } }
            /// <summary>
            /// Gets a value indicating whether this instance is being updated on the remote server.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is being updated on the remote server; otherwise, <c>false</c>.
            /// </value>
            public bool IsUpdating { get { return HasFlags(EventFlagType.Updating); } }
            /// <summary>
            /// Gets a value indicating whether this instance is being released on the remote server.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is being released on the remote server; otherwise, <c>false</c>.
            /// </value>
            public bool IsReleasing { get { return HasFlags(EventFlagType.Releasing); } }
            /// <summary>
            /// Gets a value indicating whether this instance is being downloaded from the remote server.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is being released on the remote server; otherwise, <c>false</c>.
            /// </value>
            public bool IsDownloading { get { return HasFlags(EventFlagType.Downloading); } }
            /// <summary>
            /// Gets a value indicating whether this instance is loaded.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
            /// </value>
            public bool IsLoaded { get { return HasFlags(EventFlagType.Loaded); } }
            /// <summary>
            /// Gets a value indicating whether this instance is canceled.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is canceled; otherwise, <c>false</c>.
            /// </value>
            public bool IsCanceled { get { return HasFlags(EventFlagType.Canceled); } }
            /// <summary>
            /// Gets a value indicating whether this instance has timed-out.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance has timed-out; otherwise, <c>false</c>.
            /// </value>
            public bool IsTimeout { get { return HasFlags(EventFlagType.Timeout); } }
            /// <summary>
            /// Gets a value indicating whether this instance has errored.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has errored; otherwise, <c>false</c>.
            /// </value>
            public bool IsError { get { return HasFlags(EventFlagType.Error); } }

            /// <summary>
            /// Gets a value indicating whether the local asset/resource is newer.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the local asset/resource is newer; otherwise, <c>false</c>.
            /// </value>
            public bool IsLocalNewer { get { return HasFlags(StatusType.LocalIsNewer); } }
            /// <summary>
            /// Gets a value indicating whether the local asset/resource is older.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the local asset/resource is older; otherwise, <c>false</c>.
            /// </value>
            public bool IsLocalOlder { get { return HasFlags(StatusType.LocalIsOlder); } }
            /// <summary>
            /// Gets a value indicating whether the local asset/resource is the same age (version) as the remote asset/resource.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the local asset/resource is the same age (version) as the remote asset/resource; otherwise, <c>false</c>.
            /// </value>
            public bool IsLocalSameAsRemote { get { return HasFlags(StatusType.LocalMatchesRemote); } }
            /// <summary>
            /// Gets a value indicating whether the remote asset/resource exists.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the remote asset/resource exists; otherwise, <c>false</c>.
            /// </value>
            /// <remarks>Only use the value contained herein if <see cref="IsRemoteExistantKnown"/> is <c>true</c> because this data is 
            /// only accurate at that time.</remarks>
            public bool IsRemoteExistant { get { return HasFlags(StatusType.RemoteExists); } }
            /// <summary>
            /// Gets a value indicating whether <see cref="IsRemoteExistant"/> is accurate.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if <see cref="IsRemoteExistant"/> is accurate; otherwise, <c>false</c>.
            /// </value>
            public bool IsRemoteExistantKnown { get { return HasFlags(StatusType.RemoteExistsIsKnown); } }
            /// <summary>
            /// Gets a value indicating whether the local asset/resource exists.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the local asset/resource exists; otherwise, <c>false</c>.
            /// </value>
            public bool IsLocalExistant { get { return HasFlags(StatusType.LocalExists); } }
            /// <summary>
            /// Gets a value indicating whether <see cref="IsLocalExistant"/> is accurate.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if <see cref="IsLocalExistant"/> is accurate; otherwise, <c>false</c>.
            /// </value>
            public bool IsLocalExistantKnown { get { return HasFlags(StatusType.LocalExistsIsKnown); } }

            /// <summary>
            /// Gets a value indicating whether this instance can save.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can save; otherwise, <c>false</c>.
            /// </value>
            public bool CanSave { get { return IsLocalExistantKnown && IsLocalExistant && IsLocalNewer; } }
            /// <summary>
            /// Gets a value indicating whether this instance can get (download).
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance can get (download); otherwise, <c>false</c>.
            /// </value>
            public bool CanGet { get { return IsRemoteExistantKnown && IsRemoteExistant && (IsLocalOlder || IsLocalSameAsRemote); } }





            /// <summary>
            /// Initializes a new instance of the <see cref="TVIState"/> class.
            /// </summary>
            /// <param name="resource">The <see cref="Common.Storage.Version"/>.</param>
            public State(Common.Storage.Version resource)
            {
                _eventFlags = EventFlagType.None;
                _status = StatusType.None;
                _direction = DirectionType.None;
                _resource = resource;
            }

            /// <summary>
            /// Determines whether this instance has the specified event flag(s).
            /// </summary>
            /// <param name="flag">The flag.</param>
            /// <returns>
            ///   <c>true</c> if this instance has the specified event flag(s); otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>Multiple flags can be specified; however, this will only return <c>true</c> in the event that they all match.</remarks>
            private bool HasFlags(EventFlagType flag)
            {
                return (_eventFlags & flag) == flag;
            }

            /// <summary>
            /// Determines whether this instance has the specified resource status flag(s).
            /// </summary>
            /// <param name="flag">The flag.</param>
            /// <returns>
            ///   <c>true</c> if this instance has the specified resource status flag(s); otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>Multiple flags can be specified; however, this will only return <c>true</c> in the event that they all match.</remarks>
            private bool HasFlags(StatusType flag)
            {
                return (_status & flag) == flag;
            }

            /// <summary>
            /// Determines whether this instance has the specified resource direction flag(s).
            /// </summary>
            /// <param name="flag">The flag.</param>
            /// <returns>
            ///   <c>true</c> if this instance has the specified resource direction flag(s); otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>Multiple flags can be specified; however, this will only return <c>true</c> in the event that they all match.</remarks>
            private bool HasFlags(DirectionType flag)
            {
                return (_direction & flag) == flag;
            }

            /// <summary>
            /// Gets the event flags.
            /// </summary>
            /// <returns></returns>
            public EventFlagType GetEventFlags()
            {
                return _eventFlags;
            }

            /// <summary>
            /// Gets the resource status flags.
            /// </summary>
            /// <returns></returns>
            public StatusType GetStatusFlags()
            {
                return _status;
            }

            /// <summary>
            /// Gets the resource direction flags.
            /// </summary>
            /// <returns></returns>
            public DirectionType GetDirectionFlags()
            {
                return _direction;
            }

            /// <summary>
            /// Sets the event flags.
            /// </summary>
            /// <param name="flags">The event flags.</param>
            public void SetFlags(EventFlagType flags)
            {
                _eventFlags = flags;
            }

            /// <summary>
            /// Sets the resource status flags.
            /// </summary>
            /// <param name="flags">The flags.</param>
            public void SetFlags(StatusType flags)
            {
                _status = flags;
            }

            /// <summary>
            /// Sets the resource direction flags.
            /// </summary>
            /// <param name="flags">The flags.</param>
            public void SetFlags(DirectionType flags)
            {
                _direction = flags;
            }

            /// <summary>
            /// Clears the specified resource event flags.
            /// </summary>
            /// <param name="flags">The flags.</param>
            public void ClearFlags(StatusType flags)
            {
                _status &= ~flags;
            }

            /// <summary>
            /// Clears the specified resource event flags.
            /// </summary>
            /// <param name="flags">The flags.</param>
            public void ClearFlags(EventFlagType flags)
            {
                _eventFlags &= ~flags;
            }

            /// <summary>
            /// Clears the specified resource direction flags.
            /// </summary>
            /// <param name="flags">The flags.</param>
            public void ClearFlags(DirectionType flags)
            {
                _direction &= ~flags;
            }

            /// <summary>
            /// Sets the event flag to canceled.
            /// </summary>
            public void Cancel()
            {
                _eventFlags |= EventFlagType.Canceled;
            }

            /// <summary>
            /// Updates the event flags.
            /// </summary>
            /// <param name="isCreating">if set to <c>true</c> is creating.</param>
            /// <param name="isUpdating">if set to <c>true</c> is updating.</param>
            /// <param name="isReleasing">if set to <c>true</c> is releasing.</param>
            /// <param name="isDownloading">if set to <c>true</c> is downloading.</param>
            /// <param name="isLoaded">if set to <c>true</c> is loaded.</param>
            /// <param name="isCanceled">if set to <c>true</c> is canceled.</param>
            /// <param name="isTimeout">if set to <c>true</c> has timed-out.</param>
            /// <param name="isError">if set to <c>true</c> has errored.</param>
            public void UpdateEvent(bool isCreating, bool isUpdating, bool isReleasing, bool isDownloading,
                bool isLoaded, bool isCanceled, bool isTimeout, bool isError)
            {
                // Directional Event Flags

                if (isCreating) _eventFlags |= EventFlagType.Creating;
                else _eventFlags &= ~EventFlagType.Creating;

                if (isUpdating) _eventFlags |= EventFlagType.Updating;
                else _eventFlags &= ~EventFlagType.Updating;

                if (isReleasing) _eventFlags |= EventFlagType.Releasing;
                else _eventFlags &= ~EventFlagType.Releasing;

                if (isDownloading) _eventFlags |= EventFlagType.Downloading;
                else _eventFlags &= ~EventFlagType.Downloading;

                // Non-Directional Event Flags

                if (isLoaded) _eventFlags |= EventFlagType.Loaded;
                else _eventFlags &= ~EventFlagType.Loaded;

                if (isCanceled) _eventFlags |= EventFlagType.Canceled;
                else _eventFlags &= ~EventFlagType.Canceled;

                if (isTimeout) _eventFlags |= EventFlagType.Timeout;
                else _eventFlags &= ~EventFlagType.Timeout;

                if (isError) _eventFlags |= EventFlagType.Error;
                else _eventFlags &= ~EventFlagType.Error;
            }

            /// <summary>
            /// Updates the direction.
            /// </summary>
            /// <param name="isLocalToRemote">if set to <c>true</c> is local to remote.</param>
            /// <param name="isRemoteToLocal">if set to <c>true</c> is remote to local.</param>
            public void UpdateDirection(bool isLocalToRemote, bool isRemoteToLocal)
            {
                if (isLocalToRemote && isRemoteToLocal)
                    throw new ArgumentException("Direction cannot be both ways");

                _direction = DirectionType.None;

                if (isLocalToRemote) _direction = DirectionType.LocalToRemote;
                else if (isRemoteToLocal) _direction = DirectionType.RemoteToLocal;
                else _direction = DirectionType.None;
            }

            /// <summary>
            /// Updates the resource status flags.
            /// </summary>
            /// <param name="localIsNewer">The local asset/resource is newer than the remote.</param>
            /// <param name="localIsOlder">The local asset/resource is older than the remote.</param>
            /// <param name="localMatchesRemote">The local asset/resource is the same age (version) as the remote.</param>
            /// <param name="remoteExists">The remote asset/resource exists.</param>
            /// <param name="localExists">The local asset/resource exists.</param>
            public void UpdateResourceStatus(bool? localIsNewer, bool? localIsOlder,
                bool? localMatchesRemote, bool? remoteExists, bool? localExists)
            {
                _status = SetResourceStatusFlag(_status,
                    StatusType.LocalIsNewer, localIsNewer);

                _status = SetResourceStatusFlag(_status,
                    StatusType.LocalIsOlder, localIsOlder);

                _status = SetResourceStatusFlag(_status,
                    StatusType.LocalMatchesRemote, localMatchesRemote);

                if (remoteExists.HasValue)
                {
                    _status |= StatusType.RemoteExistsIsKnown;
                    _status = SetResourceStatusFlag(_status,
                        StatusType.RemoteExists, remoteExists);
                }

                if (localExists.HasValue)
                {
                    _status |= StatusType.LocalExistsIsKnown;
                    _status = SetResourceStatusFlag(_status,
                        StatusType.LocalExists, localExists);
                }
            }

            /// <summary>
            /// Sets the resource status flag.
            /// </summary>
            /// <param name="currentFlags">The current flags.</param>
            /// <param name="flagToToggle">The flag to toggle.</param>
            /// <param name="newValue">The new value.</param>
            /// <returns>The new resource status</returns>
            private StatusType SetResourceStatusFlag(
                StatusType currentFlags,
                StatusType flagToSet,
                bool? newValue)
            {
                if (!newValue.HasValue) return currentFlags;

                if (newValue.Value) return currentFlags | flagToSet;
                else return currentFlags & ~flagToSet;
            }
        }

        public delegate void EventDelegate(State state);
        public delegate void StatusUpdateDelegate(State state, string status);
        public event EventDelegate OnCancel;
        public event EventDelegate OnReload;
        public event EventDelegate OnItemSelect;
        public event EventDelegate OnRelease;
        public event StatusUpdateDelegate OnStatusUpdate;

        /// <summary>
        /// The brush for outdated assets.
        /// </summary>
        Brush _outdatedBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)); // Yellow
        /// <summary>
        /// The brush for assets in a state of error or timeout
        /// </summary>
        Brush _errorBrush = new SolidColorBrush(Color.FromArgb(75, 255, 0, 0)); // Red
        /// <summary>
        /// The brush for assets that need saved to the server.
        /// </summary>
        Brush _needUpdatedBrush = new SolidColorBrush(Color.FromArgb(75, 0, 255, 0)); // Green
        /// <summary>
        /// The brush for assets normally.
        /// </summary>
        Brush _normalBrush = Brushes.Transparent;

        private List<Guid> _ignoreFileChangesForGuids = null;

        System.Windows.Controls.Primitives.StatusBarItem _sbItem;

        public Version Selected
        {
            get
            {
                if (ResourceTree.SelectedItem == null) return null;
                return ((State)((TreeViewItem)ResourceTree.SelectedItem).Tag).Resource;
            }
            set
            {
                int i = FindTreeViewItemIndex(value);
                if (i<0) throw new IndexOutOfRangeException("The specified resource cannot be located within the tree.");
                SetResourceTreeSelectedIndex(i);
            }
        }

        public ResourceTreeView()
        {
            InitializeComponent();
            _ignoreFileChangesForGuids = new List<Guid>();
        }

        public void RegisterStatusBarItem(System.Windows.Controls.Primitives.StatusBarItem sbitem)
        {
            _sbItem = sbitem;
        }

        public bool ResourceExistsInTree(Guid guid)
        {
            return (FindTreeViewItem(guid) != null);
        }

        public bool ResourceExistsInTree(Version resource)
        {
            return (FindTreeViewItem(resource) != null);
        }

        public void LocalResourceChanged(Guid guid)
        {
            TreeViewItem tvi;

            if (_ignoreFileChangesForGuids.Contains(guid))
            {
                _ignoreFileChangesForGuids.Remove(guid);
                return;
            }

            if ((tvi = FindTreeViewItem(guid)) == null)
                throw new ArgumentException("The specified guid could not be found in the tree.");

            LocalResourceChanged(tvi);
        }

        public void LocalResourceChanged(Version resource)
        {
            TreeViewItem tvi;

            if (_ignoreFileChangesForGuids.Contains(resource.Guid))
            {
                _ignoreFileChangesForGuids.Remove(resource.Guid);
                return;
            }

            if ((tvi = FindTreeViewItem(resource)) == null)
                throw new ArgumentException("The specified resource could not be found in the tree.");

            LocalResourceChanged(tvi);
        }

        private void LocalResourceChanged(TreeViewItem tvi)
        {
            State state;

            UpdateStatus(tvi, "File needs saved to server.");

            state = (State)tvi.Tag;
            state.UpdateResourceStatus(true, false, false, null, true);
            tvi.Background = _needUpdatedBrush;
        }

        public void StartStatusCheck(Version resource)
        {
            TreeViewItem tvi;
            State state;

            if ((tvi = FindTreeViewItem(resource)) == null)
                throw new ArgumentException("The resource does not exist.");

            state = (State)tvi.Tag;
            state.UpdateDirection(false, true);
            state.UpdateEvent(false, false, false, true, true, false, false, false);

            TreeViewItemProps.SetIsCanceled(tvi, false);
            TreeViewItemProps.SetIsLoading(tvi, true);
            TreeViewItemProps.SetPercentComplete(tvi, 50);
            UpdateStatus(tvi, "Checking status.");

            state.JobType = Common.Work.Master.JobType.CheckUpdateStatus;

            SetResourceTreeSelectedIndex(-1);
        }

        public void FinishStatusCheck(Common.Work.JobResult result, string fileMd5)
        {
            TreeViewItem tvi;
            State state;

            if ((tvi = FindTreeViewItem(result.Resource)) == null)
            {
                // Item does not exist in the tree, it needs added if the job was successful
                if (result.Job.IsCancelled)
                {
                    MessageBox.Show("Actions on resource " + result.Resource.Guid.ToString("N") + " were cancelled by the user.");
                }
                else if (result.Job.IsError)
                {
                    MessageBox.Show("An error occurred while trying to check the status of the resource " + result.Resource.Guid.ToString("N") + ".");
                }
                else if (result.Job.IsTimeout)
                {
                    MessageBox.Show("Actions on resource " + result.Resource.Guid.ToString("N") + " timed out.");
                }
                else if (result.Job.IsFinished)
                {
                    tvi = new TreeViewItem();
                    state = new State(result.InputArgs.Resource);

                    tvi.Tag = state;
                    tvi.Selected += new RoutedEventHandler(TreeViewItem_Selected);

                    TreeViewItemProps.SetGuid(tvi, result.Resource.Guid.ToString("N"));
                    TreeViewItemProps.SetIsCanceled(tvi, false);
                    TreeViewItemProps.SetIsLoading(tvi, false);
                    TreeViewItemProps.SetPercentComplete(tvi, 0);

                    state.UpdateDirection(false, false);
                    state.UpdateEvent(false, false, false, false, true, false, false, false);
                    if (string.IsNullOrEmpty(result.Resource.MetaAsset.Md5)
                        || result.InputArgs.Resource.MetaAsset.Md5 != fileMd5)
                    {
                        state.UpdateResourceStatus(true, false, false, true, true);
                        UpdateStatus(tvi, "File needs saved to server.");
                        tvi.Background = _needUpdatedBrush;
                    }
                    else
                    {
                        state.UpdateResourceStatus(false, false, true, true, true);
                        UpdateStatus(tvi, "Loaded.");
                        tvi.Background = _normalBrush;
                    }

                    if (result.InputArgs.Resource.MetaAsset != null &&
                        !string.IsNullOrEmpty(result.InputArgs.Resource.MetaAsset.Title))
                        tvi.Header = result.InputArgs.Resource.MetaAsset.Title;
                    else
                        tvi.Header = result.InputArgs.Resource.Guid;

                    AddContextMenu(tvi);

                    ResourceTree.Items.Add(tvi);
                }
                else
                {
                    throw new Exception("Unhandled event");
                }
            }
            else
            {
                // Item does exist in the tree, we just need to update
                state = (State)tvi.Tag;
                TreeViewItemProps.SetIsLoading(tvi, false);
                TreeViewItemProps.SetPercentComplete(tvi, 0);
                state.UpdateDirection(false, false);

                if (result.Job.IsCancelled)
                {
                    TreeViewItemProps.SetIsCanceled(tvi, true);
                    UpdateStatus(tvi, "Status check canceled by user.");
                    state.UpdateEvent(false, false, false, false, true, true, false, false);
                    tvi.Background = _errorBrush;
                }
                else if (result.Job.IsError)
                {
                    TreeViewItemProps.SetIsCanceled(tvi, true);
                    UpdateStatus(tvi, "Error: failed status check.");
                    state.UpdateEvent(false, false, false, false, true, false, false, true);
                    tvi.Background = _errorBrush;
                }
                else if (result.Job.IsTimeout)
                {
                    TreeViewItemProps.SetIsCanceled(tvi, true);
                    UpdateStatus(tvi, "Error: timeout occurred performing status check.");
                    state.UpdateEvent(false, false, false, false, true, false, true, false);
                    tvi.Background = _errorBrush;
                }
                else if (result.Job.IsFinished)
                {
                    TreeViewItemProps.SetIsCanceled(tvi, true);
                    
                    state.UpdateEvent(false, false, false, false, true, false, false, false);
                    if (string.IsNullOrEmpty(result.Resource.MetaAsset.Md5)
                        || result.Resource.MetaAsset.Md5 != fileMd5)
                    {
                        state.UpdateResourceStatus(true, false, false, true, true);
                        UpdateStatus(tvi, "File needs saved to server.");
                        tvi.Background = _needUpdatedBrush;
                    }
                    else
                    {
                        state.UpdateResourceStatus(false, false, true, true, true);
                        UpdateStatus(tvi, "Loaded.");
                        tvi.Background = _normalBrush;
                    }
                }
                else
                {
                    throw new Exception("Unhandled event");
                }
            }

            SetResourceTreeSelectedIndex(-1);
        }

        public void StartDownload(Version resource, Common.Work.Master.JobType jobType)
        {
            if (FindTreeViewItem(resource) != null)
                throw new ArgumentException("The resource already exists.");

            State state;
            TreeViewItem tvi;

            tvi = new TreeViewItem();

            state = new State(resource);
            state.UpdateDirection(false, true);
            state.UpdateEvent(false, false, false, true, false, false, false, false);
            state.UpdateResourceStatus(false, true, false, true, false);

            tvi.Tag = state;
            tvi.Selected += new RoutedEventHandler(TreeViewItem_Selected);

            TreeViewItemProps.SetGuid(tvi, resource.MetaAsset.Guid.ToString("N"));
            TreeViewItemProps.SetIsCanceled(tvi, false);
            TreeViewItemProps.SetIsLoading(tvi, true);
            TreeViewItemProps.SetPercentComplete(tvi, 0);
            UpdateStatus(tvi, "Downloading resource.");

            state.JobType = jobType;

            if (resource.MetaAsset != null && !string.IsNullOrEmpty(resource.MetaAsset.Title))
                tvi.Header = resource.MetaAsset.Title;
            else
                tvi.Header = resource.Guid;

            AddContextMenu(tvi);

            ResourceTree.Items.Add(tvi);

            // Add twice because we need to ignore both the meta and data assets
            _ignoreFileChangesForGuids.Add(resource.Guid);
            _ignoreFileChangesForGuids.Add(resource.Guid);

            SetResourceTreeSelectedIndex(-1);
        }

        public void FinishDownload(Common.Work.JobResult result)
        {
            TreeViewItem tvi;
            State state;

            if ((tvi = FindTreeViewItem(result.Resource)) == null)
                throw new ArgumentException("The resource does not exist.");

            state = (State)tvi.Tag;
            state.Resource = result.Resource;

            state.UpdateDirection(false, false);

            if (result.Job.IsCancelled)
            {
                TreeViewItemProps.SetIsCanceled(tvi, true);
                TreeViewItemProps.SetIsLoading(tvi, false);
                UpdateStatus(tvi, "Download canceled by user.");
                state.UpdateEvent(false, false, false, false, false, true, false, false);
                tvi.Background = _errorBrush;
            }
            else if (result.Job.IsError)
            {
                TreeViewItemProps.SetIsCanceled(tvi, true);
                TreeViewItemProps.SetIsLoading(tvi, false);
                UpdateStatus(tvi, "An error occurred, please retry");
                state.UpdateEvent(false, false, false, false, false, false, false, true);
                tvi.Background = _errorBrush;
            }
            else if (result.Job.IsFinished)
            {
                // Finished has a race condition
                // The progress event could fire into here if it finished before the report was sent, thus it
                // would fall into finished and trigger here then the finished event would trigger a work
                // report and fire here.  Thus, we need to check to ensure it only happens once by checking 
                lock (state)
                {
                    if (state.IsDownloading)
                    {
                        TreeViewItemProps.SetIsCanceled(tvi, false);
                        TreeViewItemProps.SetIsLoading(tvi, false);
                        TreeViewItemProps.SetPercentComplete(tvi, 0);

                        state.UpdateEvent(false, false, false, false, true, false, false, false);
                        state.UpdateResourceStatus(false, false, true, true, true);

                        tvi.Background = _normalBrush;

                        if (state.Resource.MetaAsset != null && 
                            !string.IsNullOrEmpty(state.Resource.MetaAsset.Title))
                            tvi.Header = state.Resource.MetaAsset.Title;
                        else
                            tvi.Header = state.Resource.Guid;

                        UpdateStatus(tvi, "Loaded");
                    }
                }
            }
            else if (result.Job.IsTimeout)
            {
                TreeViewItemProps.SetIsCanceled(tvi, true);
                TreeViewItemProps.SetIsLoading(tvi, false);
                UpdateStatus(tvi, "Save timed-out, please retry");
                state.UpdateEvent(false, false, false, false, false, false, true, false);
                tvi.Background = _errorBrush;
            }
            else
            {
                TreeViewItemProps.SetPercentComplete(tvi, result.Job.PercentComplete);
                UpdateStatus(tvi, "Downloading resource is " + result.Job.PercentComplete.ToString() + "% complete, " +
                                Utilities.MakeBytesHumanReadable(result.Job.BytesComplete) + " of " +
                                Utilities.MakeBytesHumanReadable(result.Job.BytesTotal) + " have been downloaded.");
            }


            TreeViewItemProps.SetIsCanceled(tvi, result.Job.IsCancelled);
            TreeViewItemProps.SetIsLoading(tvi, false);
            TreeViewItemProps.SetPercentComplete(tvi, result.Job.PercentComplete);
            UpdateStatus(tvi, "Loaded");

            state.UpdateDirection(false, false);
            if (result.Job.IsError || result.Job.IsCancelled || result.Job.IsTimeout)
                state.UpdateEvent(false, false, false, false, false, result.Job.IsCancelled,
                    result.Job.IsTimeout, result.Job.IsError);
            else
                state.UpdateEvent(false, false, false, false, true, result.Job.IsCancelled,
                    result.Job.IsTimeout, result.Job.IsError);
        }

        /// <summary>
        /// Adds a resource that exists on the local filesystem as a new node on the tree.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void AddNewExistingLocalResource(Version resource)
        {
            if (FindTreeViewItem(resource) != null)
                throw new ArgumentException("The resource already exists.");

            State state;
            TreeViewItem tvi;

            tvi = new TreeViewItem();
            state = new State(resource);

            tvi.Tag = state;
            tvi.Selected += new RoutedEventHandler(TreeViewItem_Selected);

            TreeViewItemProps.SetGuid(tvi, resource.MetaAsset.Guid.ToString("N"));
            TreeViewItemProps.SetIsCanceled(tvi, false);
            TreeViewItemProps.SetIsLoading(tvi, false);
            TreeViewItemProps.SetPercentComplete(tvi, 0);
            UpdateStatus(tvi, "Resource needs saved to server.");

            state.UpdateDirection(false, false);
            state.UpdateEvent(true, false, false, false, true, false, false, false);
            state.UpdateResourceStatus(true, false, false, false, true);

            tvi.Background = _needUpdatedBrush;

            if (resource.MetaAsset != null && !string.IsNullOrEmpty(resource.MetaAsset.Title))
                tvi.Header = resource.MetaAsset.Title;
            else
                tvi.Header = resource.Guid;


            AddContextMenu(tvi);

            ResourceTree.Items.Add(tvi);

            SetResourceTreeSelectedIndex(-1);
        }

        public State StartSaveToRemote(Version resource)
        {
            TreeViewItem tvi;
            State state;

            if ((tvi = FindTreeViewItem(resource)) == null)
                throw new ArgumentException("The resource does not exist.");

            TreeViewItemProps.SetIsLoading(tvi, true);
            TreeViewItemProps.SetPercentComplete(tvi, 0);
            state = (State)tvi.Tag;

            TreeViewItemProps.SetIsCanceled(tvi, false);
            if (state.IsRemoteExistantKnown && !state.IsRemoteExistant)
            {
                state.UpdateEvent(true, false, false, false, true, false, false, false);
                state.JobType = Common.Work.Master.JobType.CreateResource;
            }
            else
            {
                state.UpdateEvent(false, true, false, false, true, false, false, false);
                state.JobType = Common.Work.Master.JobType.SaveResource;
            }
            UpdateStatus(tvi, "Saving resource to server.");

            state.UpdateDirection(true, false);
            if (state.IsRemoteExistantKnown && !state.IsRemoteExistant)

            SetResourceTreeSelectedIndex(-1);

            return state;
        }

        public void FinishSaveToRemote(Common.Work.JobResult result)
        {
            TreeViewItem tvi;
            State state;

            // Checks for old resource existance

            if (result.InputArgs.Resource != null &&
                (tvi = FindTreeViewItem(result.InputArgs.Resource.Guid)) != null)
            {
                state = (State)tvi.Tag;
                state.Resource = result.Resource;
                TreeViewItemProps.SetGuid(tvi, result.Resource.Guid.ToString("N"));
            }
            else if ((tvi = FindTreeViewItem(result.Resource)) != null)
            {
                state = (State)tvi.Tag;
            }
            else
            {
                Common.Logger.General.Error("Unable to locate the resource in the GUI tree.");
                throw new ArgumentException("Neither the old nor new resource could be located.");
            }

            if (result.Job.IsCancelled ||
                result.Job.IsError ||
                result.Job.IsFinished ||
                result.Job.IsTimeout)
            {
                // Update resource
                state.Resource = result.Resource;
                TreeViewItemProps.SetGuid(tvi, state.Resource.Guid.ToString("N"));

                // Update direction
                state.UpdateDirection(false, false);
            }

            if (result.Job.IsCancelled)
            {
                TreeViewItemProps.SetIsCanceled(tvi, true);
                TreeViewItemProps.SetIsLoading(tvi, false);

                state.UpdateEvent(false, false, false, false, true, true, false, false);

                tvi.Background = _errorBrush;

                UpdateStatus(tvi, "Save was canceled by user.");
            }
            else if (result.Job.IsError)
            {
                TreeViewItemProps.SetIsCanceled(tvi, true);
                TreeViewItemProps.SetIsLoading(tvi, false);

                state.UpdateEvent(false, false, false, false, true, false, false, true);

                tvi.Background = _errorBrush;

                UpdateStatus(tvi, "An error occurred, please retry");
            }
            else if (result.Job.IsFinished)
            {
                // Finished has a race condition
                // The progress event could fire into here if it finished before the report was sent, thus it
                // would fall into finished and trigger here then the finished event would trigger a work
                // report and fire here.  Thus, we need to check to ensure it only happens once by checking 
                lock (state)
                {
                    if ((state.IsUpdating || state.IsCreating))
                    {
                        TreeViewItemProps.SetIsCanceled(tvi, false);
                        TreeViewItemProps.SetIsLoading(tvi, false);
                        TreeViewItemProps.SetPercentComplete(tvi, 0);

                        state.UpdateEvent(false, false, false, false, true, false, false, false);
                        state.UpdateResourceStatus(false, false, true, true, true);

                        tvi.Background = _normalBrush;

                        UpdateStatus(tvi, "Loaded.");
                    }
                }
            }
            else if (result.Job.IsTimeout)
            {
                TreeViewItemProps.SetIsCanceled(tvi, true);
                TreeViewItemProps.SetIsLoading(tvi, false);

                state.UpdateEvent(false, false, false, false, true, false, true, false);

                tvi.Background = _errorBrush;

                UpdateStatus(tvi, "Save timed-out, please retry");
            }
            else
            {
                TreeViewItemProps.SetPercentComplete(tvi, result.Job.PercentComplete);
                UpdateStatus(tvi, "Uploading resource is " + result.Job.PercentComplete.ToString() + "% complete, " +
                                Utilities.MakeBytesHumanReadable(result.Job.BytesComplete) + " of " +
                                Utilities.MakeBytesHumanReadable(result.Job.BytesTotal) + " have been uploaded.");
            }
        }
        
        public void RemoveLocalResource(Version resource)
        {
            State state;
            TreeViewItem tvi;

            if ((tvi = FindTreeViewItem(resource)) == null)
            {
                Common.Logger.General.Error("Unable to locate the resource in the GUI tree.");
                throw new ArgumentException("Could not locate resource in tree.");
            }

            state = (State)tvi.Tag;
            state.UpdateDirection(false, false);
            state.UpdateEvent(false, false, true, false, false, false, false, false);

            if (ResourceTree.Items.Contains(tvi))
                ResourceTree.Items.Remove(tvi);
        }

        public Version GetResourceFromTree(Guid guid)
        {
            TreeViewItem tvi = FindTreeViewItem(guid);

            if (tvi == null) return null;

            return ((State)tvi.Tag).Resource;
        }

        private void AddContextMenu(TreeViewItem tvi)
        {
            tvi.ContextMenu = new ContextMenu();
            tvi.ContextMenu.PlacementTarget = tvi;
            tvi.ContextMenu.IsOpen = false;

            System.Windows.Controls.MenuItem mi1 = new MenuItem();
            mi1.Header = "Release";
            mi1.Click += new RoutedEventHandler(MenuItem_Click);

            //System.Windows.Controls.MenuItem mi2 = new MenuItem();
            //mi2.Header = "IsLoading

            tvi.ContextMenu.Items.Add(mi1);
        }

        /// <summary>
        /// Handles the Click event of the btnCancelLoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void btnCancelLoad_Click(object sender, RoutedEventArgs e)
        {
            State state;
            TreeViewItem tviOwner;
            Button btnSender = (Button)e.OriginalSource;

            if (btnSender != null)
            {
                tviOwner = (TreeViewItem)btnSender.Tag;
                if (tviOwner != null)
                {
                    if (tviOwner.Tag != null)
                    {
                        state = (State)tviOwner.Tag;

                        state.Cancel();

                        TreeViewItemProps.SetIsCanceled(tviOwner, true);
                        TreeViewItemProps.SetIsLoading(tviOwner, false);
                        TreeViewItemProps.SetStatus(tviOwner, "Canceled by user.");

                        tviOwner.Background = _errorBrush;

                        if (OnCancel != null) OnCancel((State)tviOwner.Tag);
                    }
                }
            }

            SetResourceTreeSelectedIndex(-1);
        }

        /// <summary>
        /// Handles the Click event of the btnReload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void btnReload_Click(object sender, RoutedEventArgs e)
        {
            State state;
            TreeViewItem tviOwner;
            Button btnSender = (Button)e.OriginalSource;

            if (btnSender != null)
            {
                tviOwner = (TreeViewItem)btnSender.Tag;
                if (tviOwner != null)
                {
                    if (tviOwner.Tag != null)
                    {
                        state = (State)tviOwner.Tag;
                        
                        TreeViewItemProps.SetIsCanceled(tviOwner, false);
                        TreeViewItemProps.SetIsLoading(tviOwner, true);
                        TreeViewItemProps.SetStatus(tviOwner, "Starting reload...");

                        tviOwner.Background = _normalBrush;

                        if (OnReload != null) OnReload(state);
                    }
                }
            }

            SetResourceTreeSelectedIndex(-1);
        }

        /// <summary>
        /// Handles the Selected event of the TreeViewItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
            State state = (State)tvi.Tag;

            if (_sbItem == null) throw new InvalidOperationException("RegisterStatusBarItem must be called first.");
            _sbItem.Content = TreeViewItemProps.GetStatus(tvi);

            // Outdated Brush = Local resource is outdated (older than remote)
            // Error Brush = Something bad happened and the last action failed
            // Need Updated Brush = Local resource is newer than remote and needs saved to the server
            // Normal Brush = Local matches remote

            if (OnItemSelect != null) OnItemSelect(state);
        }

        /// <summary>
        /// Handles the Click event of the MenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi;
            State state;
            MenuItem mi = (MenuItem)sender;
            DependencyObject dobj = VisualTreeHelper.GetParent((DependencyObject)sender);

            while (dobj.GetType() != typeof(System.Windows.Controls.ContextMenu))
            {
                dobj = VisualTreeHelper.GetParent((DependencyObject)dobj);
            }

            tvi = (TreeViewItem)((System.Windows.Controls.ContextMenu)dobj).PlacementTarget;
            state = (State)tvi.Tag;

            switch ((string)mi.Header)
            {
                case "Release":
                    // Releases the lock on the resource at the server and removes the resource from 
                    // the local filesystem also removing it from the ResourceTree
                    ResourceTree.Items.Remove(tvi);
                    if (OnRelease != null) OnRelease(state);
                    break;
                //case "Lock":
                //    // Applies a lock on the resource at the server and downloads an updated MetaAsset
                //    LockResource(tvi);
                //    break;
                //case "Unlock":
                //    // Releases a lock on the resource at the server and downloads an updated MetaAsset
                //    UnlockResource(tvi);
                //    break;
                default:
                    throw new Exception("Unknown context menu item.");
            }
        }
                
        /// <summary>
        /// Finds the index of the <see cref="Version"/>.
        /// </summary>
        /// <param name="resource">The <see cref="Version"/>.</param>
        /// <returns>An integer value of the index if found; otherwise, -1.</returns>
        private int FindTreeViewItemIndex(Version resource)
        {
            TreeViewItem tvi;

            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                tvi = (TreeViewItem)ResourceTree.Items[i];
                if (tvi.Tag != null) // If null, then Tag has not been set, which means that LoadResource has not been called and thus, it cannot be the one we want
                {
                    if (((State)tvi.Tag).Resource.Guid == resource.Guid)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the specified <see cref="FullAsset"/> in the tree.
        /// </summary>
        /// <param name="resource">The <see cref="Version"/>.</param>
        /// <returns>
        /// A <see cref="TreeViewItem"/> if located; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// Runs on the UI thread.
        /// </remarks>
        private TreeViewItem FindTreeViewItem(Version resource)
        {
            TreeViewItem tvi;

            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                tvi = (TreeViewItem)ResourceTree.Items[i];
                if (tvi.Tag != null) // If null, then Tag has not been set, which means that LoadResource has not been called and thus, it cannot be the one we want
                {
                    if (((State)tvi.Tag).Resource.Guid == resource.Guid)
                        return tvi;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the specified <see cref="Guid"/> in the tree.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/>.</param>
        /// <returns>A <see cref="TreeViewItem"/> if located; otherwise, <c>null</c>.</returns>
        /// <remarks>Runs on the UI thread.</remarks>
        private TreeViewItem FindTreeViewItem(Guid guid)
        {
            TreeViewItem tvi;

            // Locate the resource in the tree
            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                tvi = (TreeViewItem)ResourceTree.Items[i];
                if (tvi.Tag != null) // If null, then Tag has not been set, which means that LoadResource has not been called and thus, it cannot be the one we want
                {
                    if (((State)tvi.Tag).Resource.Guid == guid)
                    {
                        return tvi;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the specified <see cref="string"/> in the tree.
        /// </summary>
        /// <param name="header">A string representing the Guid in the header.</param>
        /// <returns>A <see cref="TreeViewItem"/> if located; otherwise, <c>null</c>.</returns>
        /// <remarks>Runs on the UI thread.</remarks>
        private TreeViewItem FindTreeViewItem(string header)
        {
            TreeViewItem tvi;

            // Locate the resource in the tree
            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                tvi = (TreeViewItem)ResourceTree.Items[i];
                if (tvi.Tag != null) // If null, then Tag has not been set, which means that LoadResource has not been called and thus, it cannot be the one we want
                {
                    if (tvi.Header.ToString() == header)
                    {
                        return tvi;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Updates the status of a <see cref="TreeViewItem"/>.
        /// </summary>
        /// <param name="tvi">The <see cref="TreeViewItem"/> to update.</param>
        /// <param name="status">The new status.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void UpdateStatus(TreeViewItem tvi, string status)
        {
            TreeViewItemProps.SetStatus(tvi, status);
            if (OnStatusUpdate != null) OnStatusUpdate((State)tvi.Tag, status);
        }

        /// <summary>
        /// Selects the <see cref="TreeViewItem"/> at the specified index in the tree.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetResourceTreeSelectedIndex(int index)
        {
            DependencyObject dObject;

            if (index < 0)
            {
                if (ResourceTree.Items.Count >= 0)
                {
                    // If dObject can be set to the 0 index and selected then we know no other item can be selected
                    // So, then we just set it to unselected and presto, nothing is selected.
                    // If no items exist, then nothing can be selected anyway.
                    dObject = ResourceTree.ItemContainerGenerator.ContainerFromIndex(0);
                    if (dObject != null)
                    {
                        ((TreeViewItem)dObject).IsSelected = true;
                        ((TreeViewItem)dObject).IsSelected = false;
                    }
                }

                if (OnItemSelect != null) OnItemSelect(null);
            }
            else
            {
                dObject = ResourceTree.ItemContainerGenerator.ContainerFromIndex(index);
                ((TreeViewItem)dObject).IsSelected = true;

                System.Reflection.MethodInfo selectMethod = typeof(TreeViewItem).GetMethod("Select",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                selectMethod.Invoke(dObject, new object[] { true });
            }
        }
    }
}
