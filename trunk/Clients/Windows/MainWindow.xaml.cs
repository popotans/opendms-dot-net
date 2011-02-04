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
using Common.Data;
using Common.Work;

namespace WindowsClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Common.Work.IWorkRequestor
    {
        // The following events were commented out because they are not used

        //delegate void LoaderDelegate(TreeViewItem tvi, FullAsset fullAsset,
        //    ResourceFunctionDelegate actResourceFunction,
        //    AddSubItemDelegate actAddSubItem);
        //delegate void ResourceFunctionDelegate(FullAsset fullAsset);
        //delegate void AddSubItemDelegate(TreeViewItem tviParent, FullAsset fullAsset);
        //delegate void UpdateProgressOnUIDelegate(DataAsset sender, int percentComplete, bool isDone);
        //delegate void FileSystemEventDelegate(string filepath, string oldfilepath);

        /// <summary>
        /// Represents the method that handles a resource event.
        /// </summary>
        /// <param name="tvi">The <see cref="TreeViewItem"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        delegate void ResourceDelegate(TreeViewItem tvi, FullAsset fullAsset);

        /// <summary>
        /// The brush for outdated assets.
        /// </summary>
        Brush _outdatedBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)); // Yellow
        /// <summary>
        /// The brush for assets in a state of error.
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

        /// <summary>
        /// The <see cref="Guid"/> of the item whos properties are currently being displayed in the status bar.
        /// </summary>
        Guid _statusBarItemGuid;
        /// <summary>
        /// A reference to the <see cref="Master"/>.
        /// </summary>
        Master _workMaster;
        /// <summary>
        /// A reference to the <see cref="FileSystemWatcher"/> monitoring the file system.
        /// </summary>
        FileSystemWatcher _fsWatcher;
        /// <summary>
        /// A collection of filepaths to ignore when detected for creation.
        /// </summary>
        List<string> _fsWatcherCreateIgnore;
        /// <summary>
        /// A collection of filepaths to ignore when detected for change.
        /// </summary>
        List<string> _fsWatcherChangeIgnore;
        /// <summary>
        /// An array of directories where creation, change and deletion are allowed by the user without using the GUI.
        /// </summary>
        private static string[] _allowedDirectories = { @"C:\ClientDataStore\data\", @"C:\ClientDataStore\metadata\", @"C:\ClientDataStore\settings\" };

        /// <summary>
        /// A reference to a global <see cref="Settings"/>.
        /// </summary>
        public static Settings Settings;
        /// <summary>
        /// A reference to a global <see cref="ErrorManager"/>.
        /// </summary>
        public static Common.ErrorManager ErrorManager;
        /// <summary>
        /// A reference to a global <see cref="Common.FileSystem.IO"/>.
        /// </summary>
        public static Common.FileSystem.IO FileSystem;
        /// <summary>
        /// A reference to a global <see cref="Common.Logger"/> to document general events.
        /// </summary>
        public static Common.Logger GeneralLogger;
        /// <summary>
        /// A reference to a global <see cref="Common.Logger"/> to document network events.
        /// </summary>
        public static Common.Logger NetworkLogger;




        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            bool testTouch = Common.ServerSettings.Instance.IsLoaded;

            Settings = Settings.Load();
            Common.ErrorManager.UpdateUI actErrorUpdateUI = ErrorUpdateUI;
            GeneralLogger = new Common.Logger("GeneralLog.txt");
            NetworkLogger = new Common.Logger("NetworkLog.txt");
            FileSystem = new Common.FileSystem.IO(Settings.StorageLocation, GeneralLogger);

            ErrorManager = new Common.ErrorManager(actErrorUpdateUI, GeneralLogger);
            _statusBarItemGuid = Guid.Empty;
            _workMaster = new Master(ErrorManager, FileSystem, GeneralLogger, NetworkLogger);
            _fsWatcherCreateIgnore = new List<string>();
            _fsWatcherChangeIgnore = new List<string>();
            _fsWatcher = new FileSystemWatcher(MainWindow.Settings.StorageLocation);
            _fsWatcher.IncludeSubdirectories = true;
            _fsWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            //_fsWatcher.Deleted += new FileSystemEventHandler(FS_Deleted);
            //_fsWatcher.Created += new FileSystemEventHandler(FS_Created);
            //_fsWatcher.Changed += new FileSystemEventHandler(FS_Changed);
            //_fsWatcher.Renamed += new RenamedEventHandler(FS_Renamed);
            //_fsWatcher.EnableRaisingEvents = true;
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
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
            ResourceTree.Items.Clear();
            //CreateTestSearchForm();
            //CreateTestResources();

            if (!Settings.SettingsFileExists)
            {
                SettingsWindow win = new SettingsWindow();
                win.ShowDialog();
            }

            LoadLocalResources();
        }
        
        // UI
        //private void FS_RenamedOnUI(string filepath, string oldfilepath)
        //{
        //    TreeViewItem tvi;
        //    FullAsset fullAsset;
        //    string name = System.IO.Path.GetFileNameWithoutExtension(oldfilepath);

        //    if ((tvi = FindTreeViewItem(name)) != null)
        //    {
        //        fullAsset = (LocalResource)tvi.Tag;
        //        localResource.SetFilename(System.IO.Path.GetFileNameWithoutExtension(filepath));
        //        tvi.Header = localResource.Filename;
        //    }
        //}

        //// BG
        //void FS_Renamed(object sender, RenamedEventArgs e)
        //{
        //    FileSystemEventDelegate actUpdateUI = FS_RenamedOnUI;
        //    Dispatcher.BeginInvoke(actUpdateUI, e.FullPath, e.OldFullPath);
        //}

        //// UI
        //private void FS_ChangedOnUI(string filepath, string oldfilepath)
        //{
        //    TreeViewItem tvi;
        //    string name = System.IO.Path.GetFileNameWithoutExtension(filepath);

        //    if ((tvi = FindTreeViewItem(name)) != null)
        //    {
        //        tvi.Background = _needUpdatedBrush;
        //        UpdateStatus(tvi, "File needs saved to server");
        //    }
        //}

        //// BG
        //void FS_Changed(object sender, FileSystemEventArgs e)
        //{
        //    string guid = e.Name;

        //    if (guid.Contains(System.IO.Path.DirectorySeparatorChar))
        //        guid = guid.Substring(guid.IndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
        //    if (guid.Contains('.'))
        //        guid = guid.Substring(0, guid.IndexOf('.'));

        //    lock (_fsWatcherChangeIgnore)
        //    {
        //        if (_fsWatcherChangeIgnore.Contains(guid))
        //        {
        //            _fsWatcherChangeIgnore.Remove(guid);
        //            return;
        //        }
        //    }

        //    FileSystemEventDelegate actUpdateUI = FS_ChangedOnUI;
        //    Dispatcher.BeginInvoke(actUpdateUI, e.FullPath, null);
        //}

        //// UI
        //private void FS_CreatedOnUI(string filepath, string oldfilepath)
        //{
        //    bool pass = false;
        //    TreeViewItem tvi;
        //    LocalResource localResource;
        //    DirectoryInfo di = new DirectoryInfo(System.IO.Path.GetDirectoryName(filepath));
        //    string name = System.IO.Path.GetFileNameWithoutExtension(filepath);
        //    string dataPath = MainWindow.Settings.StorageLocation + new AssetType(AssetType.Data).VirtualPath + System.IO.Path.DirectorySeparatorChar;



        //    if (File.Exists(filepath))
        //    { // File
        //        if (di.FullName + "\\" != dataPath)
        //        {
        //            if (di.Name != "settings")
        //            {
        //                File.Delete(filepath);
        //                MessageBox.Show("Files can only be created within " + dataPath, "Error Adding Resource", MessageBoxButton.OK, MessageBoxImage.Error);
        //                return;
        //            }
        //        }
        //    }
        //    else if (Directory.Exists(filepath))
        //    { // Directory
        //        for (int i = 0; i < _allowedDirectories.Length; i++)
        //        {
        //            if (_allowedDirectories[i] == filepath + "\\")
        //            {
        //                pass = true;
        //                break;
        //            }
        //        }

        //        if (!pass)
        //        {
        //            Directory.Delete(filepath);
        //            MessageBox.Show("Directories cannot be created within " + Settings.StorageLocation, "Error Adding Resource", MessageBoxButton.OK, MessageBoxImage.Error);
        //            return;
        //        }
        //    }
        //    else
        //        return; // do nothing if it no longer exists, the program would have taken care of it

        //    // Does it already exist?            
        //    if ((tvi = FindTreeViewItem(name)) == null)
        //    {
        //        // Doesn't exist - needs added
        //        localResource = new LocalResource(name, Settings.StorageLocation);
        //        tvi = new TreeViewItem();
        //        tvi.Header = localResource.Filename;
        //        tvi.Tag = localResource;
        //        tvi.Background = _needUpdatedBrush;
        //        TreeViewItemProps.SetIsCanceled(tvi, false);
        //        TreeViewItemProps.SetIsLoaded(tvi, true);
        //        TreeViewItemProps.SetIsLoading(tvi, false);
        //        UpdateStatus(tvi, "File needs saved to server");
        //        ResourceTree.Items.Add(tvi);
        //    }
        //    else
        //    {
        //        // Does exist but the file was just created
        //        if (((LocalResource)tvi.Tag).Guid != Guid.Empty)
        //        { // Created by the system as it is being downloaded
        //        }
        //        else
        //        { // Created by the user
        //            localResource = new LocalResource(name, Settings.StorageLocation);
        //            tvi.Header = localResource.Filename;
        //            tvi.Tag = localResource;
        //            tvi.Background = _needUpdatedBrush;
        //            TreeViewItemProps.SetIsCanceled(tvi, false);
        //            TreeViewItemProps.SetIsLoaded(tvi, true);
        //            TreeViewItemProps.SetIsLoading(tvi, false);
        //            UpdateStatus(tvi, "File needs saved to server");
        //        }
        //    }
        //}

        //// BG
        //void FS_Created(object sender, FileSystemEventArgs e)
        //{
        //    string guid = e.Name;

        //    if (Directory.Exists(e.FullPath))
        //    {
        //        if (e.Name == "meta" || e.Name == "metadata" ||
        //            e.Name == "data" || e.Name == "settings")
        //            return;
        //        else
        //        {
        //            Directory.Delete(e.FullPath);
        //            return;
        //        }
        //    }
            
        //    if (guid.Contains(System.IO.Path.DirectorySeparatorChar))
        //        guid = guid.Substring(guid.IndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
        //    if (guid.Contains('.'))
        //        guid = guid.Substring(0, guid.IndexOf('.'));

        //    lock (_fsWatcherCreateIgnore)
        //    {
        //        if (_fsWatcherCreateIgnore.Contains(guid))
        //        {
        //            _fsWatcherCreateIgnore.Remove(guid);
        //            return;
        //        }
        //    }

        //    FileSystemEventDelegate actUpdateUI = FS_CreatedOnUI;
        //    Dispatcher.BeginInvoke(actUpdateUI, e.FullPath, null);
        //}

        //// UI
        //private void FS_DeletedOnUI(string filepath, string oldfilepath)
        //{
        //    TreeViewItem tvi;
        //    LocalResource localResource;
        //    string name = System.IO.Path.GetFileNameWithoutExtension(filepath);

        //    if ((tvi = FindTreeViewItem(name)) != null)
        //    {
        //        // TODO: Need to release any checkouts on the server
        //        localResource = (LocalResource)tvi.Tag;
        //        ResourceTree.Items.Remove(tvi);
        //        if (File.Exists(localResource.MetaAsset.LocalPath))
        //            File.Delete(localResource.MetaAsset.LocalPath);
        //        if (File.Exists(localResource.DataAsset.LocalPath))
        //            File.Delete(localResource.DataAsset.LocalPath);
        //        TreeViewItemProps.SetIsLoaded(tvi, false);
        //        UpdateStatus(tvi, "");
        //    }
        //}

        //// BG
        //void FS_Deleted(object sender, FileSystemEventArgs e)
        //{
        //    FileSystemEventDelegate actUpdateUI = FS_DeletedOnUI;
        //    Dispatcher.BeginInvoke(actUpdateUI, e.FullPath, null);
        //}

        // UI
        //void CreateTestResources()
        //{
        //    // 05d51eda96a269c9d474578cb300242a
        //    LocalResource resource1 = new LocalResource(new Guid("05d51eda96a269c9d474578cb300242a"), new ETag("2"), Settings.StorageLocation);
        //    List<string> tags1 = new List<string>();
        //    tags1.Add("tag1");
        //    tags1.Add("tag2");
        //    tags1.Add("tag3");
        //    Dictionary<string, object> dict1 = new Dictionary<string,object>();
        //    dict1.Add("prop1", 1);
        //    dict1.Add("prop2", DateTime.Now);
        //    dict1.Add("prop3", "lucas");
        //    MetaAsset ma = MetaAsset.Create(resource1, Settings.StorageLocation, resource1.ETag, new Guid("15d51eda96a269c9d474578cb300242a"), 
        //        new Guid("25d51eda96a269c9d474578cb300242a"), 1297920, ".exe", DateTime.Now, DateTime.Now, DateTime.Now, "Test 1", tags1, dict1);
        //    ma.SaveToLocal();

        //    // 05d51eda96a269c9d474578cb300242b
        //    LocalResource resource2 = new LocalResource(new Guid("05d51eda96a269c9d474578cb300242b"), new ETag("1"), Settings.StorageLocation);
        //    List<string> tags2 = new List<string>();
        //    tags2.Add("tag1");
        //    tags2.Add("tag2");
        //    tags2.Add("tag3");
        //    Dictionary<string, object> dict2 = new Dictionary<string, object>();
        //    dict2.Add("prop1", 1);
        //    dict2.Add("prop2", DateTime.Now);
        //    dict2.Add("prop3", "lucas");
        //    ma = MetaAsset.Create(resource2, Settings.StorageLocation, resource2.ETag, new Guid("15d51eda96a269c9d474578cb300242b"), Guid.Empty,
        //        1297920, ".exe", DateTime.Now, DateTime.Now, DateTime.Now, "Test 1", tags2, dict2);
        //    ma.SaveToLocal();

        //    // 05d51eda96a269c9d474578cb300242c
        //    LocalResource resource3 = new LocalResource(new Guid("05d51eda96a269c9d474578cb300242c"), new ETag("1"), Settings.StorageLocation);
        //    List<string> tags3 = new List<string>();
        //    tags3.Add("tag1");
        //    tags3.Add("tag2");
        //    tags3.Add("tag3");
        //    Dictionary<string, object> dict3 = new Dictionary<string, object>();
        //    dict3.Add("prop1", 1);
        //    dict3.Add("prop2", DateTime.Now);
        //    dict3.Add("prop3", "lucas");
        //    ma = MetaAsset.Create(resource3, Settings.StorageLocation, resource3.ETag, Guid.Empty, Guid.Empty,
        //        1297920, ".exe", DateTime.Now, DateTime.Now, DateTime.Now, "Test 1", tags3, dict3);
        //    ma.SaveToLocal();

        //    // 05d51eda96a269c9d474578cb300242d
        //    LocalResource resource4 = new LocalResource(new Guid("05d51eda96a269c9d474578cb300242d"), new ETag("1"), Settings.StorageLocation);
        //    List<string> tags4 = new List<string>();
        //    tags4.Add("tag1");
        //    tags4.Add("tag2");
        //    tags4.Add("tag3");
        //    Dictionary<string, object> dict4 = new Dictionary<string, object>();
        //    dict4.Add("prop1", 1);
        //    dict4.Add("prop2", DateTime.Now);
        //    dict4.Add("prop3", "lucas");
        //    ma = MetaAsset.Create(resource4, Settings.StorageLocation, resource4.ETag, Guid.Empty, Guid.Empty,
        //        26703, ".odt", DateTime.Now, DateTime.Now, DateTime.Now, "Test 1", tags4, dict4);
        //    ma.SaveToLocal();
        //}

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

            sf.SaveToFile("settings\\searchform.xml", FileSystem, GeneralLogger, true);
        }

        /// <summary>
        /// Loads all resources on the local file system.
        /// </summary>
        /// <remarks>Runs on the UI thread.</remarks>
        void LoadLocalResources()
        {
            MetaAsset ma;
            FullAsset fullAsset;
            Guid guid = Guid.Empty;
            string temp;
            LoadResourceJob.UpdateUIDelegate actUpdateUI = CheckETagStatus;
            string[] files = FileSystem.GetFiles(AssetType.Meta.VirtualPath);

            for (int i = 0; i < files.Length; i++)
            {
                temp = files[i];
                files[i] = System.IO.Path.GetFileNameWithoutExtension(files[i]);

                try { guid = new Guid(files[i]); }
                catch
                {
                    MessageBox.Show("A file was found in the storage location that cannot exist.\r\nPlease remove: " + temp);
                }

                if (guid != Guid.Empty)
                {
                    ma = new MetaAsset(guid, FileSystem, GeneralLogger);
                    if (ma.Load(GeneralLogger))
                    {
                        fullAsset = new FullAsset(ma, new DataAsset(ma, FileSystem, GeneralLogger));
                        _workMaster.AddJob(this, Master.JobType.GetETag, fullAsset, actUpdateUI, 100000);
                    }
                    else
                    {
                        MessageBox.Show("The resource with id " + guid.ToString("N") + " failed to load, please verify the formatting of its meta data.");
                    }
                }
            }
        }

        // Region commented as it is no longer used but I am not yet comfortable removing the code completely, thus it was made into a region :)
        #region Obsolete Commented Code

        ///// <summary>
        ///// Calls <see cref="M:LoadMetaCallback(AssetJobBase job, FullAsset fullAsset)"/>.
        ///// </summary>
        ///// <param name="job">The <see cref="JobBase"/>.</param>
        ///// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        ///// <remarks>Runs on the UI thread.</remarks>
        //void LoadMetaCallback(JobBase job, FullAsset fullAsset)
        //{
        //    LoadMetaCallback((AssetJobBase)job, fullAsset);
        //}

        ///// <summary>
        ///// Called when a job has terminated on a <see cref="MetaAsset"/>.
        ///// </summary>
        ///// <param name="job">The <see cref="AssetJobBase"/>.</param>
        ///// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        ///// <remarks>Runs on the UI thread.</remarks>
        //void LoadMetaCallback(AssetJobBase job, FullAsset fullAsset)
        //{
        //    TreeViewItem tvi;
        //    TVIState tviState;
        //    FullAsset localFullAsset;

        //    tvi = FindTreeViewItem(fullAsset);

        //    if (tvi != null)
        //    {
        //        tviState = (TVIState)tvi.Tag;
        //        localFullAsset = tviState.FullAsset;
        //        TreeViewItemProps.SetIsLoading(tvi, false);

        //        if (job.IsCancelled)
        //        {
        //            tviState.UpdateEvent(false, false, true, false, false);
        //            TreeViewItemProps.SetIsCanceled(tvi, true);
        //            UpdateStatus(tvi, "Action cancelled by user");
        //        }
        //        else if (job.IsFinished)
        //        {
        //            tvi.Background = _normalBrush;
        //            tviState.UpdateEvent(false, true, false, false, false);
        //            TreeViewItemProps.SetPercentComplete(tvi, 100);

        //            if (localFullAsset.MetaAsset.ETag.IsOlder(job.FullAsset.MetaAsset.ETag))
        //            {
        //                tvi.Background = _outdatedBrush;
        //                tviState.UpdateResourceStatus(false, true, false, true, true);
        //                UpdateStatus(tvi, "A newer version of this resource exists");
        //            }
        //            else
        //            { // Impossible for remote to be newer, cus we just downloaded it
        //                tviState.UpdateResourceStatus(false, false, true, true, true);
        //                UpdateStatus(tvi, "Loaded");
        //            }
        //        }
        //        else if (job.IsTimeout)
        //        {
        //            tvi.Background = _errorBrush;
        //            tviState.UpdateEvent(false, false, false, true, false);
        //            UpdateStatus(tvi, "Error: Timeout");
        //        }
        //        else if (job.IsError)
        //        {
        //            tviState.UpdateEvent(false, false, false, false, true);
        //            UpdateStatus(tvi, "Error");
        //        }
        //        else
        //        {
        //            throw new Exception("Unhandled event");
        //        }
        //    }
        //    else
        //    {
        //        if (job.IsCancelled)
        //        {
        //            MessageBox.Show("Actions on resource " + job.FullAsset.Guid.ToString("N") + " were cancelled by the user.");
        //        }
        //        else if (job.IsFinished)
        //        {
        //            tvi = AddTreeResource(fullAsset, false, true);
        //            tviState = (TVIState)tvi.Tag;
        //            tvi.Background = _normalBrush;
        //            TreeViewItemProps.SetPercentComplete(tvi, 100);
        //            TreeViewItemProps.SetIsLoading(tvi, false);

        //            if (job.FullAsset.MetaAsset.ETag == null)
        //            {
        //                // Remote resource does not exist on the server if the ETag is null
        //                tvi.Background = _needUpdatedBrush;
        //                tviState.UpdateResourceStatus(true, false, false, false, true);
        //                UpdateStatus(tvi, "File needs saved to server");
        //            }
        //            else if (fullAsset.MetaAsset.ETag.IsOlder(job.FullAsset.MetaAsset.ETag))
        //            {
        //                tvi.Background = _outdatedBrush;
        //                tviState.UpdateResourceStatus(false, true, false, true, true);
        //                UpdateStatus(tvi, "A newer version of this resource exists");
        //            }
        //            else
        //            {
        //                tviState.UpdateResourceStatus(false, false, true, true, true);
        //                UpdateStatus(tvi, "Loaded");
        //            }
        //        }
        //        else if (job.IsTimeout)
        //        {
        //            MessageBox.Show("Actions on resource " + job.FullAsset.Guid.ToString("N") + " timed out.");
        //        }
        //        else if (job.IsError)
        //        {

        //        }
        //        else
        //        {
        //            throw new Exception("Unhandled event");
        //        }
        //    }
        //}

        #endregion

        /// <summary>
        /// Calls <see cref="M:CheckETagStatus(GetETagJob job, FullAsset fullAsset)"/>.
        /// </summary>
        /// <param name="job">The <see cref="JobBase"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void CheckETagStatus(JobBase job, FullAsset fullAsset)
        {
            CheckETagStatus((GetETagJob)job, (FullAsset)fullAsset);
        }

        /// <summary>
        /// Called when a <see cref="GetETagJob"/> has terminated.
        /// </summary>
        /// <param name="job">The <see cref="GetETagJob"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void CheckETagStatus(GetETagJob job, FullAsset fullAsset)
        {
            TVIState tviState;
            TreeViewItem tvi = FindTreeViewItem(fullAsset);

            if (tvi == null)
            {
                if (job.IsCancelled)
                {
                    MessageBox.Show("Actions on resource " + job.FullAsset.Guid.ToString("N") + " were cancelled by the user.");
                }
                else if (job.IsError)
                {
                    MessageBox.Show("An error occurred while trying to check the status of the resource " + job.FullAsset.Guid.ToString("N") + ".");
                }
                else if (job.IsFinished)
                {
                    tvi = AddTreeResource(fullAsset, false, true);
                    tviState = (TVIState)tvi.Tag;
                    TreeViewItemProps.SetPercentComplete(tvi, 100);
                    TreeViewItemProps.SetIsLoading(tvi, false);

                    if (fullAsset.MetaAsset.ETag.IsOlder(job.ETag))
                    {
                        // The local version is older
                        tvi.Background = _outdatedBrush;
                        tviState.UpdateResourceStatus(false, true, false, true, true);
                        UpdateStatus(tvi, "A newer version of this resource exists");
                    }
                    else if (fullAsset.MetaAsset.ETag.IsNewer(job.ETag))
                    {
                        // The local version is newer
                        tvi.Background = _needUpdatedBrush;
                        if (job.ETag.Value == "0")
                            tviState.UpdateResourceStatus(true, false, false, false, true);
                        else
                            tviState.UpdateResourceStatus(true, false, false, true, true);
                        UpdateStatus(tvi, "File needs saved to server");
                    }
                    else if (fullAsset.MetaAsset.ETag.Equals(job.ETag))
                    {
                        tvi.Background = _normalBrush;
                        tviState.UpdateResourceStatus(false, false, true, true, true);
                        UpdateStatus(tvi, "Loaded");
                    }
                    else if (job.IsTimeout)
                    {
                        MessageBox.Show("Actions on resource " + job.FullAsset.Guid.ToString("N") + " timed out.");
                    }
                    else if (job.IsError)
                    {

                    }
                    else
                    {
                        throw new Exception("Unhandled event");
                    }
                }
            }
            else
            {
                tviState = (TVIState)tvi.Tag;
                TreeViewItemProps.SetIsLoading(tvi, false);

                if (job.IsCancelled)
                {
                    TreeViewItemProps.SetIsCanceled(tvi, true);
                    tviState.UpdateEvent(false, false, true, false, false);
                    UpdateStatus(tvi, "Action cancelled by user");
                }
                else if (job.IsFinished)
                {
                    tvi.Background = _normalBrush;
                    tviState.UpdateEvent(false, true, false, false, false);
                    TreeViewItemProps.SetPercentComplete(tvi, 100);

                    if (fullAsset.MetaAsset.ETag.IsOlder(job.ETag))
                    {
                        // Local is older
                        tvi.Background = _outdatedBrush;
                        tviState.UpdateResourceStatus(false, true, false, true, true);
                        UpdateStatus(tvi, "A newer version of this resource exists");
                    }
                    else if (fullAsset.MetaAsset.ETag.IsNewer(job.ETag))
                    {
                        // Local is newer
                        tvi.Background = _needUpdatedBrush;
                        if (job.ETag.Value == "0")
                            tviState.UpdateResourceStatus(true, false, false, false, true);
                        else
                            tviState.UpdateResourceStatus(true, false, false, true, true);
                    }
                    else
                    {
                        tviState.UpdateResourceStatus(false, false, true, true, true);
                        UpdateStatus(tvi, "Loaded");
                    }
                }
                else if (job.IsTimeout)
                {
                    tvi.Background = _errorBrush;
                    tviState.UpdateEvent(false, false, false, true, false);
                    UpdateStatus(tvi, "Error: Timeout");
                }
                else if (job.IsError)
                {
                    tviState.UpdateEvent(false, false, false, false, true);
                    UpdateStatus(tvi, "Error");
                }
                else
                {
                    throw new Exception("Unhandled event");
                }
            }
        }
        
        /// <summary>
        /// Adds a Resource to the TreeView.
        /// </summary>
        /// <param name="fullAsset">The <see cref="FullAsset"/> to add to the TreeView.</param>
        /// <param name="isLoading">If set to <c>true</c> the resource is loading from the remote host.</param>
        /// <param name="isLoaded">True if the Resource is considered loaded (up-to-date)</param>
        /// <returns>
        /// A TreeViewItem representing the same as added to the TreeView
        /// </returns>
        /// <remarks>Runs on the UI thread.</remarks>
        TreeViewItem AddTreeResource(FullAsset fullAsset, bool isLoading, bool isLoaded)
        {
            TreeViewItem tvi;

            tvi = new TreeViewItem();
            tvi.Selected += new RoutedEventHandler(TreeViewItem_Selected);
            tvi.Header = fullAsset.Guid.ToString("N");
            tvi.Tag = new TVIState(fullAsset);
            ((TVIState)tvi.Tag).UpdateEvent(isLoading, isLoaded, false, false, false);
            TreeViewItemProps.SetGuid(tvi, fullAsset.Guid.ToString("N"));
            TreeViewItemProps.SetIsLoading(tvi, isLoading);
            TreeViewItemProps.SetIsCanceled(tvi, false);
            if (isLoaded)
            {
                TreeViewItemProps.SetPercentComplete(tvi, 100);
                UpdateStatus(tvi, "Loaded");
            }
            else
            {
                TreeViewItemProps.SetPercentComplete(tvi, 0);
                UpdateStatus(tvi, "Not Loaded");
            }

            ResourceTree.Items.Add(tvi);
            
            return tvi;
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
            TVIState tviState = (TVIState)tvi.Tag;

            _statusBarItemGuid = tviState.FullAsset.Guid;
            SBItem.Content = TreeViewItemProps.GetStatus(tvi);

            // Outdated Brush = Local resource is outdated (older than remote)
            // Error Brush = Something bad happened and the last action failed
            // Need Updated Brush = Local resource is newer than remote and needs saved to the server
            // Normal Brush = Local matches remote

            /* if outdated -> disable save, enable get
             * else if error...
             * We ran into a problem, if error, what was the previous state???
             * Need to implement a new state tracking class.
             */

            if (tviState.IsLocalOlder)
            {
                BtnSaveSelected.IsEnabled = false;
                BtnGetSelected.IsEnabled = true;
            }
            else if (tviState.IsLocalNewer)
            {
                BtnSaveSelected.IsEnabled = true;
                BtnGetSelected.IsEnabled = false;
            }
            else if (tviState.IsLocalSameAsRemote)
            {
                BtnSaveSelected.IsEnabled = false;
                BtnGetSelected.IsEnabled = false;
            }
            else
                throw new Exception("Unknown state");

        }

        /// <summary>
        /// Handles the Click event of the btnCancelLoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void btnCancelLoad_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = (Button)e.OriginalSource;
            if (btnSender != null)
            {
                TreeViewItem tviOwner = (TreeViewItem)btnSender.Tag;
                if (tviOwner != null)
                {
                    if (tviOwner.Tag != null)
                    {
                        FullAsset fullAsset = ((TVIState)tviOwner.Tag).FullAsset;
                        UpdateStatus(tviOwner, "Cancelling...");
                        tviOwner.Background = _errorBrush;
                        _workMaster.CancelJobForResource(fullAsset);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnReload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = (Button)e.OriginalSource;
            LoadResourceJob.UpdateUIDelegate actUpdateUI = CheckETagStatus;
            if (btnSender != null)
            {
                TreeViewItem tviOwner = (TreeViewItem)btnSender.Tag;
                if (tviOwner != null)
                {
                    if (tviOwner.Tag != null)
                    {
                        TVIState tviState = (TVIState)tviOwner.Tag;
                        FullAsset fullAsset = tviState.FullAsset;
                        TreeViewItemProps.SetIsCanceled(tviOwner, false);
                        TreeViewItemProps.SetIsLoading(tviOwner, true);
                        TreeViewItemProps.SetStatus(tviOwner, "Starting reload...");
                        tviState.UpdateEvent(true, false, false, false, false);
                        tviOwner.Background = _normalBrush;
                        _workMaster.AddJob(this, Master.JobType.GetETag, fullAsset, actUpdateUI, 
                            (uint)Common.ServerSettings.Instance.NetworkTimeout);
                    }
                }
            }
        }

        /// <summary>
        /// Calls <see cref="M:LoadResourceCallback(LoadResourceJob, FullAsset)"/>.
        /// </summary>
        /// <param name="job">The <see cref="JobBase"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void LoadResourceCallback(JobBase job, FullAsset fullAsset)
        {
            LoadResourceCallback((LoadResourceJob)job, fullAsset);
        }

        /// <summary>
        /// Called when a job has terminated on a Resource.
        /// </summary>
        /// <param name="job">The <see cref="LoadResourceJob"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void LoadResourceCallback(LoadResourceJob job, FullAsset fullAsset)
        {
            TVIState tviState;
            TreeViewItem tvi;

            if ((tvi = FindTreeViewItem(fullAsset)) == null)
            {
                tvi = AddTreeResource(fullAsset, true, false);
            }

            tviState = (TVIState)tvi.Tag;
            
            if (job.IsCancelled)
            {
                tviState.UpdateEvent(false, false, true, false, false);
                TreeViewItemProps.SetIsLoading(tvi, false);
                TreeViewItemProps.SetIsCanceled(tvi, true);
                UpdateStatus(tvi, "Download was canceled by user");
            }
            else if (job.IsFinished)
            {
                // This could be used multiple times, depending on threading, should check a flag before making changes
                if(!tviState.IsLoaded)
                {
                    tvi.Background = _normalBrush;
                    tviState.UpdateEvent(false, true, false, false, false);
                    tviState.UpdateResourceStatus(false, false, true, true, true);
                    TreeViewItemProps.SetIsLoading(tvi, false);
                    TreeViewItemProps.SetIsCanceled(tvi, false);
                    TreeViewItemProps.SetPercentComplete(tvi, 100);
                    UpdateStatus(tvi, "Loaded");
                }
            }
            else if (job.IsTimeout)
            {
                tvi.Background = _errorBrush;
                tviState.UpdateEvent(false, false, false, true, false);
                TreeViewItemProps.SetIsLoading(tvi, false);
                TreeViewItemProps.SetIsCanceled(tvi, true);
                UpdateStatus(tvi, "Error: Timeout");
            }
            else
            {
                TreeViewItemProps.SetPercentComplete(tvi, job.PercentComplete);
                UpdateStatus(tvi, "Downloading resource is " + job.PercentComplete.ToString() + "% complete, " +
                                Utilities.MakeBytesHumanReadable(job.BytesComplete) + " of " +
                                Utilities.MakeBytesHumanReadable(job.BytesTotal) + " have been downloaded.");
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnGetSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnGetSelected_Click(object sender, RoutedEventArgs e)
        {
            TVIState tviState;
            TreeViewItem tvi;
            FullAsset fullAsset;
            LoadResourceJob.UpdateUIDelegate actUpdateUI = LoadResourceCallback;

            if (ResourceTree.SelectedItem == null)
            {
                MessageBox.Show("You must select a resource first.");
                return;
            }

            tvi = (TreeViewItem)ResourceTree.SelectedItem;

            if (tvi.Tag != null)
            {
                tviState = (TVIState)tvi.Tag;
                fullAsset = tviState.FullAsset;
                tviState.UpdateEvent(true, false, false, false, false);
                tviState.UpdateResourceStatus(null, null, null, true, true);
                TreeViewItemProps.SetIsCanceled(tvi, false);
                TreeViewItemProps.SetIsLoading(tvi, true);
                tvi.Background = _normalBrush;
                _workMaster.AddJob(this, Master.JobType.LoadResource, fullAsset, actUpdateUI, 150000);
            }
        }

        /// <summary>
        /// Calls <see cref="M:SaveResourceCallback(SaveResourceJob, FullAsset)"/>.
        /// </summary>
        /// <param name="job">The <see cref="JobBase"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void SaveResourceCallback(JobBase job, FullAsset fullAsset)
        {
            SaveResourceCallback((SaveResourceJob)job, fullAsset);
        }

        /// <summary>
        /// Called when the <see cref="SaveResourceJob"/> has terminated.
        /// </summary>
        /// <param name="job">The <see cref="SaveResourceJob"/>.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        void SaveResourceCallback(SaveResourceJob job, FullAsset fullAsset)
        {
            TVIState tviState;
            TreeViewItem tvi;

            if ((tvi = FindTreeViewItem((FullAsset)fullAsset)) == null)
            {
                tvi = AddTreeResource((FullAsset)fullAsset, true, false);
            }

            tviState = (TVIState)tvi.Tag;

            // Update the tviState.FullAsset reference
            tviState.FullAsset = job.FullAsset;

            if (job.IsCancelled)
            {
                tviState.UpdateEvent(false, true, true, false, false);
                TreeViewItemProps.SetIsLoading(tvi, false);
                TreeViewItemProps.SetIsCanceled(tvi, true);
                UpdateStatus(tvi, "Upload was canceled by user");
            }
            else if (job.IsFinished)
            {
                // This could be used multiple times, depending on threading, should check a flag before making changes
                if (!tviState.IsLoaded)
                {
                    tvi.Background = _normalBrush;
                    tviState.UpdateEvent(false, true, false, false, false);
                    tviState.UpdateResourceStatus(false, false, true, true, true);
                    TreeViewItemProps.SetIsLoading(tvi, false);
                    TreeViewItemProps.SetIsCanceled(tvi, false);
                    TreeViewItemProps.SetPercentComplete(tvi, 100);
                    UpdateStatus(tvi, "Loaded");
                }
            }
            else if (job.IsTimeout)
            {
                tvi.Background = _errorBrush;
                tviState.UpdateEvent(false, false, false, true, false);
                TreeViewItemProps.SetIsLoading(tvi, false);
                TreeViewItemProps.SetIsCanceled(tvi, true);
                UpdateStatus(tvi, "Error: Timeout");
            }
            else
            {
                TreeViewItemProps.SetPercentComplete(tvi, job.PercentComplete);
                UpdateStatus(tvi, "Uploading resource is " + job.PercentComplete.ToString() + "% complete, " +
                                Utilities.MakeBytesHumanReadable(job.BytesComplete) + " of " +
                                Utilities.MakeBytesHumanReadable(job.BytesTotal) + " have been uploaded.");
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnSaveSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnSaveSelected_Click(object sender, RoutedEventArgs e)
        {
            FullAsset fullAsset;
            DataAsset da;
            MetaAsset ma;
            TreeViewItem tvi;
            MetaPropWindow win;
            
            
            if (ResourceTree.SelectedItem == null)
            {
                MessageBox.Show("You must select a resource first.");
                return;
            }

            tvi = (TreeViewItem)ResourceTree.SelectedItem;

            if(tvi.Tag == null)
            {
                MessageBox.Show("Invalid Tag value.");
            }

            win = new MetaPropWindow(((TVIState)tvi.Tag).FullAsset.Guid);

            if (!win.ShowDialog().Value)
                return;

            // Reload the meta
            ma = MetaAsset.Load(((TVIState)tvi.Tag).FullAsset.Guid, FileSystem, GeneralLogger, NetworkLogger);
            da = new DataAsset(ma, FileSystem, GeneralLogger);
            fullAsset = new FullAsset(ma, da);

            ((TVIState)tvi.Tag).UpdateEvent(true, false, false, false, false);
            TreeViewItemProps.SetIsCanceled(tvi, false);
            TreeViewItemProps.SetIsLoading(tvi, true);
            tvi.Background = _normalBrush;

            _workMaster.AddJob(this, Master.JobType.SaveResource, fullAsset, SaveResourceCallback, 100000);
        }

        /// <summary>
        /// Finds the specified <see cref="FullAsset"/> in the tree.
        /// </summary>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <returns>A <see cref="TreeViewItem"/> if located; otherwise, <c>null</c>.</returns>
        /// <remarks>Runs on the UI thread.</remarks>
        TreeViewItem FindTreeViewItem(FullAsset fullAsset)
        {
            TreeViewItem tvi;

            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                tvi = (TreeViewItem)ResourceTree.Items[i];
                if (tvi.Tag != null) // If null, then Tag has not been set, which means that LoadResource has not been called and thus, it cannot be the one we want
                {
                    if (((TVIState)tvi.Tag).FullAsset.Guid == fullAsset.Guid)
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
        TreeViewItem FindTreeViewItem(Guid guid)
        {
            TreeViewItem tvi;

            // Locate the resource in the tree
            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                tvi = (TreeViewItem)ResourceTree.Items[i];
                if (tvi.Tag != null) // If null, then Tag has not been set, which means that LoadResource has not been called and thus, it cannot be the one we want
                {
                    if (((TVIState)tvi.Tag).FullAsset.Guid == guid)
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
        TreeViewItem FindTreeViewItem(string header)
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
        /// Handles the Click event of the BtnOpenSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnOpenSelected_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi;
            ResourceDelegate actOpenResource = OpenResource;

            if (ResourceTree.SelectedItem == null)
            {
                MessageBox.Show("You must select a resource first.");
                return;
            }

            tvi = (TreeViewItem)ResourceTree.SelectedItem;

            if (tvi.Tag != null)
            {
                tvi.Background = Brushes.Red;
                actOpenResource.BeginInvoke(tvi, ((TVIState)tvi.Tag).FullAsset, 
                    OpenResource_AsyncCallback, actOpenResource);
            }
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
            if(((TVIState)tvi.Tag).FullAsset.Guid == _statusBarItemGuid)
                SBItem.Content = status;
        }


        #region OpenResource

        /// <summary>
        /// Opens the resource.
        /// </summary>
        /// <param name="tvi">The <see cref="TreeViewItem"/> containing the resource.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>Runs on a background thread.</remarks>
        private void OpenResource(TreeViewItem tvi, FullAsset fullAsset)
        {
            string errorMessage;
            ResourceDelegate actCloseResource = CloseResource;

            if (!ExternalApplication.OpenFileWithDefaultApplication(fullAsset.DataAsset, out errorMessage))
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Dispatcher.BeginInvoke(actCloseResource, System.Windows.Threading.DispatcherPriority.Background, tvi, fullAsset);
        }

        /// <summary>
        /// Called when a resource is released.
        /// </summary>
        /// <param name="tvi">The <see cref="TreeViewItem"/> containing the resource.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/>.</param>
        /// <remarks>
        /// Runs on the UI thread.
        /// Can be called at any point after opening, depending on how the application handles file access.
        /// </remarks>
        private void CloseResource(TreeViewItem tvi, FullAsset fullAsset)
        {
            tvi.Background = Brushes.Transparent;
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
            GetETagJob.UpdateUIDelegate actUpdateUI = CheckETagStatus;
            for (int i = 0; i < ResourceTree.Items.Count; i++)
            {
                _workMaster.AddJob(this, Master.JobType.GetETag, 
                    ((TVIState)((TreeViewItem)ResourceTree.Items[i]).Tag).FullAsset, 
                    actUpdateUI, 1000);
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Runs on the UI thread.</remarks>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow search = new SearchWindow();
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
            if (FindTreeViewItem(guid) != null)
                return;

            LoadResourceJob.UpdateUIDelegate actUpdateUI = LoadResourceCallback;
            MetaAsset ma = new MetaAsset(guid, FileSystem, GeneralLogger);
            if(ma.ResourceExistsOnFilesystem())
                ma.Load(GeneralLogger);
            FullAsset fullAsset = new FullAsset(ma, new DataAsset(ma, FileSystem, GeneralLogger));
            //string datapath = resource.StorageLocation + new AssetType(AssetType.Data).VirtualPath + "\\";

            _fsWatcherCreateIgnore.Add(fullAsset.Guid.ToString("N")); // for meta
            _fsWatcherCreateIgnore.Add(fullAsset.Guid.ToString("N")); // for data
            _fsWatcherChangeIgnore.Add(fullAsset.Guid.ToString("N")); // for meta
            _fsWatcherChangeIgnore.Add(fullAsset.Guid.ToString("N")); // for data
            _workMaster.AddJob(this, Master.JobType.LoadResource, fullAsset, actUpdateUI, 150000);
        }

        /// <summary>
        /// WorkReport accepts a UpdateUIDelegate and its associated arguments and should handle pumping this message to the UI
        /// </summary>
        /// <param name="actUpdateUI">The method to update the UI.</param>
        /// <param name="job">The job for the method updating the UI.</param>
        /// <param name="fullAsset">The <see cref="FullAsset"/> for the method updating the UI.</param>
        /// <remarks>Runs on the job's thread.</remarks>
        public void WorkReport(JobBase.UpdateUIDelegate actUpdateUI, JobBase job, FullAsset fullAsset)
        {
            Dispatcher.BeginInvoke(actUpdateUI, job, fullAsset);
        }

        /// <summary>
        /// Selects the <see cref="TreeViewItem"/> at the specified index in the tree.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetResourceTreeSelectedIndex(int index)
        {
            DependencyObject dObject = ResourceTree.ItemContainerGenerator.ContainerFromIndex(index);

            ((TreeViewItem)dObject).IsSelected = true;

            System.Reflection.MethodInfo selectMethod = typeof(TreeViewItem).GetMethod("Select", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            selectMethod.Invoke(dObject, new object[] { true });
        }

        /// <summary>
        /// TEST - This is testing code used to generate a new full asset saving both its meta and 
        /// data assets to disk and returning the instantiated FullAsset object.
        /// </summary>
        /// <returns></returns>
        private Common.Data.FullAsset GenerateFullAsset()
        {
            Common.FileSystem.IOStream iostream;
            byte[] buffer;
            string dataRelativePath;
            Common.Data.MetaAsset ma;
            Common.Data.DataAsset da;
            List<string> tags = new List<string>();
            Dictionary<string, object> dict1 = new Dictionary<string,object>();
            
            tags.Add("tag1");
            tags.Add("tag2");
            tags.Add("tag3");
            dict1.Add("prop1", 1);
            dict1.Add("prop2", DateTime.Now);
            dict1.Add("prop3", "lucas");

            // Create new meta asset
            ma = Common.Data.MetaAsset.Create(Guid.NewGuid(), new ETag("1"), 1, 1, "lucas", DateTime.Now, 
                "Lucas", 0, null, ".txt", DateTime.Now, DateTime.Now, DateTime.Now, "Test", tags, 
                dict1, FileSystem, GeneralLogger);

            // Open the stream to create the new data asset
            dataRelativePath = Common.Data.AssetType.Data.VirtualPath + 
                System.IO.Path.DirectorySeparatorChar.ToString() + ma.GuidString + ".txt";

            iostream = FileSystem.Open(dataRelativePath, FileMode.Create, FileAccess.Write, FileShare.None,
                FileOptions.None, "WindowsClient.MainWindow.GenerateFullAsset()");

            // Write the new data asset
            buffer = System.Text.Encoding.UTF8.GetBytes("Test Document");
            iostream.Write(buffer, buffer.Length);
            FileSystem.Close(iostream);

            // Update the meta asset with md5 and length
            ma.UpdateByServer(ma.ETag, ma.MetaVersion, ma.DataVersion, ma.LockedBy, ma.LockedAt, ma.Creator, (ulong)buffer.Length,
                FileSystem.ComputeMd5(dataRelativePath), ma.Created, ma.Modified, ma.LastAccess);

            // Write the ma out to disk
            ma.Save();

            // Give us an instance of the data object representing the data asset
            da = new DataAsset(ma, FileSystem, GeneralLogger);

            // Construct the full asset
            return new FullAsset(ma, da);
        }

        /// <summary>
        /// Handles the Click event of the Btn1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            if (ResourceTree.SelectedItem == null)
            {
                MessageBox.Show("A resource must be selected.");
                return;
            }

            TVIState tviState = (TVIState)((TreeViewItem)ResourceTree.SelectedItem).Tag;
            Common.Data.MetaAsset ma = tviState.FullAsset.MetaAsset;
            LoadResourceJob.UpdateUIDelegate actUpdateUI = CheckETagStatus;

            tviState.FullAsset.MetaAsset.UpdateByServer(ma.ETag.Increment(), ma.MetaVersion + 1, 
                ma.DataVersion + 1, ma.LockedBy, ma.LockedAt, ma.Creator, ma.Length, ma.Md5, 
                ma.Created, ma.Modified, ma.LastAccess);

            ma.Save();

            _workMaster.AddJob(this, Master.JobType.GetETag, tviState.FullAsset, actUpdateUI, (uint)Common.ServerSettings.Instance.NetworkTimeout);
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
        /// Handles the Click event of the Btn2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            FullAsset fullAsset;
            LoadResourceJob.UpdateUIDelegate actUpdateUI = CheckETagStatus;

            fullAsset = GenerateFullAsset();
            _workMaster.AddJob(this, Master.JobType.GetETag, fullAsset, actUpdateUI, 100000);
        }

    }
}
