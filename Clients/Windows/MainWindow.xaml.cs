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
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.IO;
using Common.Storage;
using Common.Work;

namespace WindowsClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Common.Work.IWorkRequestor
    {
        /// <summary>
        /// This property is a temporary placeholder for a username
        /// </summary>
        public const string TEMP_USERNAME = "lucas";

        /// <summary>
        /// Represents the method that handles a file system event
        /// </summary>
        /// <param name="guid">The GUID.</param>
        delegate void FileSystemEventDelegate(Guid guid);

        /// <summary>
        /// Represents the method that handles a resource event.
        /// </summary>
        /// <param name="resource">The resource.</param>
        delegate void ResourceDelegate(Version resource);
        /// <summary>
        /// A reference to the <see cref="Master"/>.
        /// </summary>
        private Master _workMaster;
        /// <summary>
        /// A reference to the <see cref="FileSystemWatcher"/> monitoring the file system.
        /// </summary>
        private FileSystemWatcher _fsWatcher;

        private Common.CouchDB.Database _couchdb;
        private Dictionary<string, Guid> _addedFileMappings;

        /// <summary>
        /// The <see cref="Guid"/> of the item whos properties are currently being displayed in the status bar.
        /// </summary>
        private Guid _statusBarItemGuid;

        /// <summary>
        /// Provides Guid translation for different ids being received than are transmitted.
        /// </summary>
        /// <remarks>When an asset is loaded into the client, the client assigns it a random Guid and 
        /// knows it as this Guid only.  When the asset is saved to the server, the server ignores the 
        /// Guid assigned by the client, assigning its own Guid.  This property exists to allow the 
        /// client to match the key (the client assigned Guid) to determine the value (the server 
        /// assigned value).  This is horridly ugly and embarrassing, please fix me.</remarks>
        public static Dictionary<Guid, Guid> IdTranslation;

        /// <summary>
        /// A reference to a global <see cref="ErrorManager"/>.
        /// </summary>
        public static Common.ErrorManager ErrorManager;
        /// <summary>
        /// A reference to a global <see cref="Common.FileSystem.IO"/>.
        /// </summary>
        public static Common.FileSystem.IO FileSystem;
        /// <summary>
        /// A reference to a global set of loggers to document events.
        /// </summary>
        public static Common.Logger Logger;




        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // DO NOT REORDER variable instantiation unless you know what you are doing!!!

            InitializeComponent();

            // Settings should come first
            Settings.Instance = Settings.Load(Utilities.GetAppDataPath() + "Settings.xml");
            if (Settings.Instance == null)
            {
                SettingsWindow win = new SettingsWindow();
                Settings.Instance = new Settings();
                win.ShowDialog();
            }

            // File System must be set after settings
            FileSystem = new Common.FileSystem.IO(Settings.Instance.StorageLocation);

            // Logger can be started after FileSystem
            Logger = new Common.Logger(Utilities.GetAppDataPath());

            // CouchDB can be instantiated after both FileSystem and Logger
            _couchdb = new Common.CouchDB.Database(Settings.Instance.CouchDatabaseName,
                new Common.CouchDB.Server(Settings.Instance.CouchServerIp, Settings.Instance.CouchServerPort));

            Common.ErrorManager.UpdateUI actErrorUpdateUI = ErrorUpdateUI;

            ErrorManager = new Common.ErrorManager(actErrorUpdateUI);
            _statusBarItemGuid = Guid.Empty;
            _workMaster = new Master(ErrorManager, FileSystem, _couchdb);
            IdTranslation = new Dictionary<Guid, Guid>();
            _fsWatcher = new FileSystemWatcher(Settings.Instance.StorageLocation);
            _fsWatcher.IncludeSubdirectories = true;
            _fsWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fsWatcher.Changed += new FileSystemEventHandler(FS_Changed);
            _fsWatcher.EnableRaisingEvents = true;
            _addedFileMappings = new Dictionary<string, Guid>();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);


            ResourceTree.OnRelease += new ResourceTreeView.EventDelegate(ResourceTree_OnRelease);
            ResourceTree.OnCancel += new ResourceTreeView.EventDelegate(ResourceTree_OnCancel);
            ResourceTree.OnItemSelect += new ResourceTreeView.EventDelegate(ResourceTree_OnItemSelect);
            ResourceTree.OnReload += new ResourceTreeView.EventDelegate(ResourceTree_OnReload);
            ResourceTree.OnStatusUpdate += new ResourceTreeView.StatusUpdateDelegate(ResourceTree_OnStatusUpdate);

            ResourceTree.RegisterStatusBarItem(SBItem);
        }

        void ResourceTree_OnStatusUpdate(ResourceTreeView.State state, string status)
        {
            if (state.Resource.Guid == _statusBarItemGuid)
                SBItem.Content = status;
        }

        void ResourceTree_OnReload(ResourceTreeView.State state)
        {
            CheckUpdateStatusJob.UpdateUIDelegate actUpdateUI = null;
            
            switch(state.JobType)
            {
                case Master.JobType.CheckoutJob:
                    actUpdateUI = GetResourceCallback;
                    break;
                case Master.JobType.CheckUpdateStatus:
                    actUpdateUI = CheckUpdateStatus;
                    break;
                case Master.JobType.CreateResource:
                    // TODO
                    // This needs to delete the resource from CouchDB and then
                    // create the resource on CouchDB again.
                    throw new NotImplementedException();
                    break;
                case Master.JobType.GetResource:
                    actUpdateUI = GetResourceCallback;
                    break;
                case Master.JobType.ReleaseResource:
                    actUpdateUI = ReleaseResourceCallback;
                    break;
                case Master.JobType.SaveResource:
                    actUpdateUI = SaveResourceCallback;
                    break;
                default:
                    throw new InvalidOperationException("Unknown job type to retry.");
            }

            _workMaster.AddJob(new JobArgs()
            {
                CouchDB = _couchdb,
                ErrorManager = ErrorManager,
                FileSystem = FileSystem,
                JobType = state.JobType,
                RequestingUser = TEMP_USERNAME,
                Requestor = this,
                Resource = state.Resource,
                Timeout = (uint)Settings.Instance.NetworkTimeout,
                UpdateUICallback = actUpdateUI
            });
        }

        void ResourceTree_OnItemSelect(ResourceTreeView.State state)
        {
            if (state == null)
            {
                BtnOpenSelected.IsEnabled = BtnSaveSelected.IsEnabled = BtnGetSelected.IsEnabled = false;
                _statusBarItemGuid = Guid.Empty;
                SBItem.Content = "";
                return;
            }

            _statusBarItemGuid = state.Resource.Guid;


            // Outdated Brush = Local resource is outdated (older than remote)
            // Error Brush = Something bad happened and the last action failed
            // Need Updated Brush = Local resource is newer than remote and needs saved to the server
            // Normal Brush = Local matches remote

            /* if outdated -> disable save, enable get
             * else if error...
             * We ran into a problem, if error, what was the previous state???
             * Need to implement a new state tracking class.
             */

            if (state.IsLocalOlder)
            {
                BtnSaveSelected.IsEnabled = false;
                BtnGetSelected.IsEnabled = true;
            }
            else if (state.IsLocalNewer)
            {
                BtnSaveSelected.IsEnabled = true;
                BtnGetSelected.IsEnabled = false;
            }
            else if (state.IsLocalSameAsRemote)
            {
                BtnSaveSelected.IsEnabled = false;
                BtnGetSelected.IsEnabled = false;
            }
            else
                throw new Exception("Unknown state");
        }

        void ResourceTree_OnCancel(ResourceTreeView.State state)
        {
            Common.Storage.Version resource = null;
            
            // If the resource of the state exists then cancel it
            // This happens when a translation is not required - non CreateResourceJob
            if ((resource = ResourceTree.GetResourceFromTree(state.Resource.Guid)) != null)
            {
                _workMaster.CancelJobForResource(resource);
                return;
            }

            if (IdTranslation.ContainsValue(state.Resource.Guid))
            {
                Guid oldGuid = Guid.Empty;

                lock (IdTranslation)
                {
                    Dictionary<Guid, Guid>.Enumerator en = IdTranslation.GetEnumerator();
                    while (en.MoveNext())
                    {
                        if (en.Current.Value == state.Resource.Guid)
                        {
                            oldGuid = en.Current.Key;
                            break;
                        }
                    }
                }

                resource = ResourceTree.GetResourceFromTree(oldGuid);
                _workMaster.CancelJobForResource(resource);
                return;
            }

            throw new InvalidOperationException("Could not locate resource.");
        }

        void ResourceTree_OnRelease(ResourceTreeView.State state)
        {
            ReleaseResourceJob.UpdateUIDelegate actUpdateUI = ReleaseResourceCallback;

            _workMaster.AddJob(new JobArgs()
            {
                CouchDB = _couchdb,
                ErrorManager = ErrorManager,
                FileSystem = FileSystem,
                JobType = Master.JobType.ReleaseResource,
                RequestingUser = TEMP_USERNAME,
                Requestor = this,
                Resource = state.Resource,
                Timeout = 10000,
                UpdateUICallback = actUpdateUI
            });
        }

        /// <summary>
        /// Called when errors occur in other threads.
        /// </summary>
        /// <param name="errors">The errors.</param>
        void ErrorUpdateUI(List<Common.ErrorMessage> errors)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the Loaded event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void MainWindow_Loaded(object sender, RoutedEventArgs args)
        {
            //CreateTestSearchForm();
            //CreateTestResources();

            LoadLocalResources();
        }

        /// <summary>
        /// Called by <see cref="M:FS_Changed"/> to process actions on the UI thread.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> of the resource.</param>
        /// <remarks>
        /// This method is executed on the UI thread.
        /// </remarks>
        private void FS_ChangedOnUI(Guid guid)
        {
            ResourceTree.LocalResourceChanged(guid);
        }

        /// <summary>
        /// Called by the <see cref="FileSystemWatcher"/> when a file or directory within the monitored path is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.IO.RenamedEventArgs"/> instance containing the event data.</param>
        /// <remarks>This method is called on a background thread.</remarks>
        void FS_Changed(object sender, FileSystemEventArgs e)
        {
            Guid guid;

            // If not change type, get out
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            // If not existing still then get out
            if (!FileSystem.ResourceExists(FileSystem.GetRelativePathFromFullPath(e.FullPath)))
                return;

            // If not a guid then get out
            // TODO: This relies on exceptions when files are not resources...  This needs to attempt some 
            // type of regex check first then rely on parsing
            if (!Guid.TryParse(System.IO.Path.GetFileNameWithoutExtension(e.Name), out guid))
                return;            

            // Is this a resource that was changed pursuant to id translation by the server?  If yes, bail.
            if (IdTranslation.ContainsValue(guid))
                return;

            FileSystemEventDelegate actUpdateUI = FS_ChangedOnUI;
            Dispatcher.BeginInvoke(actUpdateUI, guid);
        }
        
        /// <summary>
        /// Creates the test search form and saves it to the local file system.
        /// </summary>
        void CreateTestSearchForm()
        {
            Common.NetworkPackage.SearchForm sf = new Common.NetworkPackage.SearchForm();

            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Text, "Id", "$guid", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Text, "Extension", "$extension", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.TextCollection, "Tags", "$tags", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Date, "Created", "$created", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Date, "Modified", "$modified", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Date, "Last Access", "$lastaccess", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Text, "User Field 1", "prop1", ""));
            sf.Add(new Common.NetworkPackage.FormProperty(Common.NetworkPackage.FormProperty.SupportedDataType.Date, "User Field 2", "prop2", ""));

            sf.SaveToFile("settings\\searchform.xml", FileSystem, true);
        }

        /// <summary>
        /// Loads all resources on the local file system.
        /// </summary>
        /// <remarks>Runs on the UI thread.</remarks>
        void LoadLocalResources()
        {
            MetaAsset ma;
            Version resource = null;
            Guid guid = Guid.Empty;
            string temp;

            CheckUpdateStatusJob.UpdateUIDelegate actUpdateUI = CheckUpdateStatus;
            string[] files = FileSystem.GetFiles(Common.FileSystem.Path.RelativeMetaPath);

            for (int i = 0; i < files.Length; i++)
            {
                temp = files[i];
                files[i] = System.IO.Path.GetFileNameWithoutExtension(files[i]);

                try { guid = new Guid(files[i]); }
                catch
                {
                    guid = Guid.Empty;
                }

                if (guid != Guid.Empty)
                {
                    ma = new MetaAsset(guid, _couchdb);

                    if (ma.LoadFromLocal(null, ma.RelativePath, FileSystem))
                    {
                        resource = new Version(ma, _couchdb);
                        _workMaster.AddJob(new JobArgs()
                        {
                            CouchDB = _couchdb,
                            ErrorManager = ErrorManager,
                            FileSystem = FileSystem,
                            JobType = Master.JobType.CheckUpdateStatus,
                            RequestingUser = TEMP_USERNAME,
                            Requestor = this,
                            Resource = resource,
                            Timeout = 100000,
                            UpdateUICallback = actUpdateUI
                        });
                    }
                    else
                    {
                        MessageBox.Show("The resource with id " + guid.ToString("N") + " failed to load, please verify the formatting of its meta data.");
                    }
                }
            }
        }

        /// <summary>
        /// Called when a <see cref="CheckUpdateStatus"/> has terminated.
        /// </summary>
        /// <param name="result">The <see cref="JobResult"/>.</param>
        /// <remarks>
        /// Runs on the UI thread.
        /// </remarks>
        void CheckUpdateStatus(JobResult result)
        {
            string localResourceMd5 = null;

            if (result.Job.IsFinished)
            {
                localResourceMd5 = new Common.FileSystem.DataResource(result.Resource.DataAsset, FileSystem).ComputeMd5();
                ResourceTree.FinishStatusCheck(result, localResourceMd5);
            }
            else
                ResourceTree.FinishStatusCheck(result, null);
        }

        /// <summary>
        /// Called when a job has terminated on a Resource.
        /// </summary>
        /// <param name="result">The <see cref="JobResult"/>.</param>
        /// <remarks>
        /// Runs on the UI thread.
        /// </remarks>
        void ReleaseResourceCallback(JobResult result)
        {
            if (result.Job.IsFinished)
            {
                ResourceTree.RemoveLocalResource(result.Resource);
                FileSystem.Delete(result.Resource.MetaAsset.RelativePath);
                FileSystem.Delete(result.Resource.DataAsset.RelativePath);
            }
        }
        
        /// <summary>
        /// Called when a job has terminated on a Resource.
        /// </summary>
        /// <param name="job">The <see cref="LoadResourceJob"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void GetResourceCallback(JobResult result)
        {
            ResourceTree.FinishDownload(result);
        }

        /// <summary>
        /// Handles the Click event of the BtnGetSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnGetSelected_Click(object sender, RoutedEventArgs e)
        {
            GetResourceJob.UpdateUIDelegate actUpdateUI = GetResourceCallback;

            if (ResourceTree.Selected == null)
            {
                MessageBox.Show("You must select a resource first.");
                return;
            }

            ResourceTree.StartDownload(ResourceTree.Selected, Master.JobType.GetResource);
            
            _workMaster.AddJob(new JobArgs()
            {
                CouchDB = _couchdb,
                ErrorManager = ErrorManager,
                FileSystem = FileSystem,
                JobType = Master.JobType.GetResource,
                RequestingUser = TEMP_USERNAME,
                Requestor = this,
                Resource = ResourceTree.Selected,
                Timeout = 150000,
                UpdateUICallback = actUpdateUI
            });
        }

        /// <summary>
        /// Called when the <see cref="CreateResourceJob"/> has terminated.
        /// </summary>
        /// <param name="result">The <see cref="JobResult"/>.</param>
        /// <remarks>
        /// Runs on the UI thread.
        /// </remarks>
        void CreateResourceCallback(JobResult result)
        {
            ResourceTree.FinishSaveToRemote(result);
        }

        /// <summary>
        /// Called when the <see cref="SaveResourceJob"/> has terminated.
        /// </summary>
        /// <param name="result">The <see cref="JobResult"/>.</param>
        /// <remarks>
        /// Runs on the UI thread.
        /// </remarks>
        void SaveResourceCallback(JobResult result)
        {
            ResourceTree.FinishSaveToRemote(result);
        }

        /// <summary>
        /// Handles the Click event of the BtnSaveSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnSaveSelected_Click(object sender, RoutedEventArgs e)
        {
            MetaPropWindow win;
            ResourceTreeView.State state;
                        
            if (ResourceTree.Selected == null)
            {
                MessageBox.Show("You must select a resource first.");
                return;
            }

            win = new MetaPropWindow(ResourceTree.Selected.Guid, FileSystem, _couchdb);

            if (!win.ShowDialog().Value)
                return;
             
            // Reload the meta
            ResourceTree.Selected.MetaAsset.LoadFromLocal(null,
                Common.FileSystem.Path.RelativeMetaPath +
                ResourceTree.Selected.Guid.ToString("N") + ".xml", FileSystem);

            lock (IdTranslation)
            {
                IdTranslation.Add(ResourceTree.Selected.Guid, Guid.Empty);
            }

            state = ResourceTree.StartSaveToRemote(ResourceTree.Selected);

            // If this resource does not exist on the server then create, else update
            if (state.IsCreating)
                _workMaster.AddJob(new JobArgs()
                {
                    CouchDB = _couchdb,
                    ErrorManager = ErrorManager,
                    FileSystem = FileSystem,
                    JobType = Master.JobType.CreateResource,
                    RequestingUser = TEMP_USERNAME,
                    Requestor = this,
                    Resource = state.Resource,
                    Timeout = 100000,
                    UpdateUICallback = CreateResourceCallback
                });
            else
                _workMaster.AddJob(new JobArgs()
                {
                    CouchDB = _couchdb,
                    ErrorManager = ErrorManager,
                    FileSystem = FileSystem,
                    JobType = Master.JobType.SaveResource,
                    RequestingUser = TEMP_USERNAME,
                    Requestor = this,
                    Resource = state.Resource,
                    Timeout = 100000,
                    UpdateUICallback = SaveResourceCallback
                });
        }


        /// <summary>
        /// Handles the Click event of the BtnOpenSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnOpenSelected_Click(object sender, RoutedEventArgs e)
        {
            ResourceDelegate actOpenResource = OpenResource;
            
            if (ResourceTree.Selected == null)
            {
                MessageBox.Show("You must select a resource first.");
                return;
            }

            actOpenResource.BeginInvoke(ResourceTree.Selected,
                    OpenResource_AsyncCallback, actOpenResource);

            ResourceTree.SetResourceTreeSelectedIndex(-1);
        }



        #region OpenResource

        /// <summary>
        /// Opens the resource.
        /// </summary>
        /// <param name="tvi">The <see cref="TreeViewItem"/> containing the resource.</param>
        /// <param name="resource">The <see cref="Version"/>.</param>
        /// <remarks>Runs on a background thread.</remarks>
        private void OpenResource(Version resource)
        {
            string errorMessage;
            ResourceDelegate actCloseResource = CloseResource;

            if (!ExternalApplication.OpenFileWithDefaultApplication(resource.DataAsset, out errorMessage))
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Dispatcher.BeginInvoke(actCloseResource, System.Windows.Threading.DispatcherPriority.Background, resource);
        }

        /// <summary>
        /// Called when a resource is released.
        /// </summary>
        /// <param name="resource">The <see cref="Version"/>.</param>
        /// <remarks>
        /// Runs on the UI thread.
        /// Can be called at any point after opening, depending on how the application handles file access.
        /// </remarks>
        private void CloseResource(Version resource)
        {
            //tvi.Background = Brushes.Transparent;
        }

        /// <summary>
        /// Called to end invoke on UI thread to process any exceptions, etc.
        /// </summary>
        /// <param name="iAR">The <see cref="IAsyncResult"/>.</param>
        private void OpenResource_AsyncCallback(IAsyncResult iAR)
        {
            // Call end invoke on UI thread to process any exceptions, etc.
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() => OpenResource_EndInvoke(iAR)));
        }

        /// <summary>
        /// Called to notify the UI of end invoke.
        /// </summary>
        /// <param name="iAR">The <see cref="IAsyncResult"/>.</param>
        private void OpenResource_EndInvoke(IAsyncResult iAR)
        {
            try
            {
                var actInvoked = (ResourceDelegate)iAR.AsyncState;
                actInvoked.EndInvoke(iAR);
            }
            catch (Exception ex)
            {
                // Probably should check for useful inner exceptions
                MessageBox.Show(string.Format("Error in ProcessEndInvokeOpenResource\r\nException:  {0}",
                    ex.Message));
            }
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the BtnExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Application.Current.Windows.Count; i++)
                Application.Current.Windows[i].Close();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the BtnRefreshETagStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnRefreshETagStatus_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdateStatusJob.UpdateUIDelegate actUpdateUI = CheckUpdateStatus;
            _workMaster.AddJob(new JobArgs()
            {
                CouchDB = _couchdb,
                ErrorManager = ErrorManager,
                FileSystem = FileSystem,
                JobType = Master.JobType.CheckUpdateStatus,
                RequestingUser = TEMP_USERNAME,
                Requestor = this,
                Resource = ResourceTree.Selected,
                Timeout = 1000,
                UpdateUICallback = actUpdateUI
            });
        }

        /// <summary>
        /// Handles the Click event of the BtnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow search = new SearchWindow(_couchdb);
            search.OnResultSelected += new SearchWindow.SearchResultHandler(search_OnResultSelected);
            search.Show();
        }

        /// <summary>
        /// Called when a search result is selected.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> of the selected resource.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void search_OnResultSelected(Guid guid)
        {
            // If the tree already has it, then ignore this after telling the user
            if (ResourceTree.ResourceExistsInTree(guid))
            {
                MessageBox.Show("The selected item is already present.");
                return;
            }

            GetResourceJob.UpdateUIDelegate actUpdateUI = GetResourceCallback;
            MetaAsset ma = new MetaAsset(guid, _couchdb);
            Version resource = new Version(ma, _couchdb);

            ResourceTree.StartDownload(resource, Master.JobType.CheckoutJob);

            _workMaster.AddJob(new JobArgs()
            {
                CouchDB = _couchdb,
                ErrorManager = ErrorManager,
                FileSystem = FileSystem,
                JobType = Master.JobType.CheckoutJob,
                ProgressMethod = JobBase.ProgressMethodType.Determinate,
                RequestingUser = TEMP_USERNAME,
                Requestor = this,
                Resource = resource,
                Timeout = 100000,
                UpdateUICallback = actUpdateUI
            });
        }
        
        /// <summary>
        /// WorkReport accepts a UpdateUIDelegate and its associated arguments and should handle pumping this message to the UI
        /// </summary>
        /// <param name="result">The <see cref="JobResult"/>.</param>
        /// <remarks>
        /// Runs on the job's thread.
        /// </remarks>
        public void WorkReport(JobResult result)
        {
            Dispatcher.BeginInvoke(result.InputArgs.UpdateUICallback, result);
        }

        public void ServerTranslation(ResourceJobBase job, Guid oldGuid, Guid newGuid)
        {
            lock (IdTranslation)
            {
                if(!IdTranslation.ContainsKey(oldGuid))
                    throw new InvalidOperationException("IdTranslation is not aware of the old guid.");
                if (IdTranslation[oldGuid] != Guid.Empty)
                    throw new InvalidOperationException("IdTranslation has already handled the old guid.");

                IdTranslation[oldGuid] = newGuid;
            }
        }

        /// <summary>
        /// TEST - This is testing code used to generate a new full asset saving both its meta and 
        /// data assets to disk and returning the instantiated FullAsset object.
        /// </summary>
        /// <returns></returns>
        private Common.Storage.Version GenerateResource()
        {
            Common.FileSystem.IOStream iostream;
            byte[] buffer;
            Common.Storage.MetaAsset ma;
            Common.Storage.DataAsset da;
            Version resource;
            List<string> tags = new List<string>();
            Dictionary<string, object> dict1 = new Dictionary<string,object>();
            
            tags.Add("tag1");
            tags.Add("tag2");
            tags.Add("tag3");
            dict1.Add("prop1", 1);
            dict1.Add("prop2", DateTime.Now);
            dict1.Add("prop3", "lucas");

            // Create new meta asset
            ma = Common.Storage.MetaAsset.Instantiate(Guid.NewGuid(), "lucas", DateTime.Now, 
                "Lucas", 0, null, ".txt", DateTime.Now, DateTime.Now, "Test", tags, 
                dict1, _couchdb);
            resource = new Version(ma, _couchdb);
            da = resource.DataAsset;

            // Open the stream to create the new data asset
            iostream = FileSystem.Open(da.RelativePath, FileMode.Create, FileAccess.Write, FileShare.None,
                FileOptions.None, "WindowsClient.MainWindow.GenerateFullAsset()");

            // Write the new data asset
            buffer = System.Text.Encoding.UTF8.GetBytes("Test Document");
            iostream.Write(buffer, buffer.Length);
            FileSystem.Close(iostream);

            // Update the meta asset with md5 and length
            ma.Length = (ulong)buffer.Length;
            ma.Md5 = FileSystem.ComputeMd5(da.RelativePath);

            // Write the ma out to disk
            ma.SaveToLocal(null, FileSystem);

            return resource;
        }

        /// <summary>
        /// Handles the Click event of the BtnSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow win = new SettingsWindow();
            win.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the BtnAddResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnAddResource_Click(object sender, RoutedEventArgs e)
        {
            Version resource;
            string dataExt;
            Guid guid = Guid.NewGuid();
            List<string> uprop = new List<string>();
            System.Windows.Forms.OpenFileDialog ofd;
            
            if(MessageBox.Show("This process will make a copy of the selected resource adding it to the repository.  Understand " +
                "that this will not affect your original file and changes made to your original file will not be made in the " + 
                "repository.  Would you like to add a resource at this time?",
                "Add a Resource", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Multiselect = false;
                
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dataExt = System.IO.Path.GetExtension(ofd.FileName);
                    while(FileSystem.ResourceExists(Common.FileSystem.Path.RelativeDataPath + guid.ToString("N") + dataExt) ||
                        FileSystem.ResourceExists(Common.FileSystem.Path.RelativeMetaPath + guid.ToString("N") + ".xml"))   
                    {
                        guid = Guid.NewGuid();
                    }
                    
                    // Create Resource
                    resource = new Version(guid, dataExt, _couchdb);

                    // Copy DataAsset
                    File.Copy(ofd.FileName, 
                    FileSystem.RootPath + Common.FileSystem.Path.RelativeDataPath + guid.ToString("N") + dataExt);

                    // Fill out the MA info
                    resource.MetaAsset.Extension = dataExt;
                    resource.MetaAsset.Created = DateTime.Now;
                    resource.MetaAsset.Creator = TEMP_USERNAME;
                    resource.MetaAsset.LastAccess = DateTime.Now;
                    resource.MetaAsset.Length = FileSystem.GetFileLength(resource.DataAsset.RelativePath);
                    resource.MetaAsset.LockedAt = DateTime.Now;
                    resource.MetaAsset.LockedBy = TEMP_USERNAME;
                    resource.MetaAsset.Title = "Temporary Title";
                    resource.MetaAsset.Md5 = FileSystem.ComputeMd5(resource.DataAsset.RelativePath);

                    resource.MetaAsset.Tags.Add("Seperate tags");
                    resource.MetaAsset.Tags.Add("by a return");

                    resource.MetaAsset.UserProperties.Add("prop1", (int)1);
                    resource.MetaAsset.UserProperties.Add("prop2", DateTime.Now);
                    resource.MetaAsset.UserProperties.Add("prop3", "string1");

                    resource.MetaAsset.SaveToLocal(null, FileSystem);

                    ResourceTree.AddNewExistingLocalResource(resource);         
                }
            }
        }
    }
}