using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenDMS.Storage.Providers;

namespace StorageTesting
{
    public partial class FrmMain : Form
    {
        private IEngine _engine;
        private IDatabase _db;
        private DateTime _start;
        private bool _isInstalled;
        private bool _isInitialized;
        private OpenDMS.Storage.Security.Session _session;

        public bool IsInstalled 
        { 
            get { return _isInstalled; }
            set
            {
                _isInstalled = true;
                ActivateTests(false, true, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false, false, false, false, false, 
                    false, false, false, false, false, false, false);
            }
        }
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                _isInitialized = true;
                ActivateTests(false, false, true, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false);
            }
        }
        public OpenDMS.Storage.Security.Session Session
        {
            get { return _session; }
            set
            {
                _session = value;
                ActivateTests(false, false, false, true, true, true, true, true, true, true,
                    true, true, true, true, true, true, true, true, true, true, true,
                    true, true, true, true, true, true, true);
            }
        }

        public FrmMain()
        {
            InitializeComponent();
            IsInitialized = false;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ActivateTests(false, false, false, false, false, false, false, false, false, false,
                false, false, false, false, false, false, false, false, false, false, false,
                false, false, false, false, false, false, false);

            _engine = new OpenDMS.Storage.Providers.CouchDB.Engine();
            _db = new OpenDMS.Storage.Providers.CouchDB.Database(
                new OpenDMS.Storage.Providers.CouchDB.Server("http", "192.168.1.111", 5984, 50000, 4096), 
                "test1");

            TxtOutput.Text = "Welcome to the OpenDMS.Storage Testing Environment.  This application allows for simple testing of the OpenDMS.Storage library and those libraries used by the OpenDMS.Storage library.\r\n\r\nThis window will display the results of the tests.\r\n\r\nSome tests must be run in certain orders.  Available tests will have enabled buttons, tests that cannot be run will be greyed out.\r\n\r\n";
            TxtOutput.Select(TxtOutput.Text.Length, 0);

            DetermineIfInstalled();
        }

        private void DetermineIfInstalled()
        {
            EngineRequest request = new EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnComplete += new EngineBase.CompletionDelegate(IsInstalled_OnComplete);
            request.OnError += new EngineBase.ErrorDelegate(IsInstalled_OnError);
            request.OnProgress += new EngineBase.ProgressDelegate(IsInstalled_OnProgress);
            request.OnTimeout += new EngineBase.TimeoutDelegate(IsInstalled_OnTimeout);
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            _start = DateTime.Now;
            WriteLine("Determining if OpenDMS.Storage has been successfully installed on the CouchDB server, please wait...");
            _engine.DetermineIfInstalled(request, @"C:\Users\Lucas\Documents\Visual Studio 2010\Projects\Test\bin\Debug\");
        }

        public void IsInstalled_OnProgress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        public void IsInstalled_OnComplete(EngineRequest request, ICommandReply reply, object result)
        {
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop - _start;


            if (!(bool)result)
            {
                BtnInstall.Invoke(new MethodInvoker(delegate { BtnInstall.Enabled = true; }));
                WriteLine("Result - OpenDMS.Storage has not been successfully installed on the CouchDB server.  Determined in " + duration.TotalMilliseconds.ToString() + "ms.");
            }
            else
            {
                IsInstalled = true;
                BtnInstall.Invoke(new MethodInvoker(delegate { BtnInstall.Enabled = false; }));
                WriteLine("Result - OpenDMS.Storage is successfully installed on the CouchDB server.  Determined in " + duration.TotalMilliseconds.ToString() + "ms.");
            }
        }

        public void IsInstalled_OnTimeout(EngineRequest request)
        {
            WriteLine("Timeout - a timeout occurred while working, this is not conclusive as to if OpenDMS.Storage is properly installed.");
        }

        public void IsInstalled_OnError(EngineRequest request, string message, Exception exception)
        {
            WriteLine("Error - an error occured while working, this is not conclusive as to if OpenDMS.Storage is properly installed.  The error message was: " + message);
        }

        public void Write(string str)
        {
            if (TxtOutput.InvokeRequired)
                TxtOutput.Invoke(new MethodInvoker(delegate { TxtOutput.Text += str; }));
            else
                TxtOutput.Text += str;
        }

        public void WriteLine(string str)
        {
            if (TxtOutput.InvokeRequired)
                TxtOutput.Invoke(new MethodInvoker(delegate { TxtOutput.Text += str + "\r\n"; }));
            else
                TxtOutput.Text += str + "\r\n";
        }

        public void Clear()
        {
            if (TxtOutput.InvokeRequired)
                TxtOutput.Invoke(new MethodInvoker(delegate { TxtOutput.Text = ""; }));
            else
                TxtOutput.Text = "";
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            Install act = new Install(this, _engine, _db);
            act.OnInstallSuccess += new Install.InstallDelegate(Install_OnInstallSuccess);
            Clear();
            act.Test();
        }

        void Install_OnInstallSuccess()
        {
            IsInstalled = true;
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void ActivateTests(bool install, bool initialize, bool authenticate,
            bool getAllGroups, bool getGroup, bool createGroup, bool modifyGroup,
            bool getAllUsers, bool getUser, bool createUser, bool modifyUser, 
            bool getResourcePermissions, bool modifyResourcePermissions, bool getGlobalPermissions,
            bool modifyGlobalPermissions, bool getResourceUsageRightsTemplate, 
            bool modifyResourceUsageRightsTemplate, bool getResource, bool getResourceReadOnly,
            bool createNewResource, bool createNewResource2, bool modifyResource, 
            bool rollbackResource, bool deleteResource, bool getVersion, bool getCurrentVersion,
            bool createNewVersion, bool modifyVersion)
        {
            if (BtnInstall.InvokeRequired)
            {
                BtnInitialize.Invoke(new MethodInvoker(delegate
                {
                    BtnInitialize.Enabled = initialize;
                    BtnInstall.Enabled = install;
                    BtnAuthenticate.Enabled = authenticate;
                    BtnGetAllGroups.Enabled = getAllGroups;
                    BtnGetGroup.Enabled = getGroup;
                    BtnCreateGroup.Enabled = createGroup;
                    BtnModifyGroup.Enabled = modifyGroup;
                    BtnGetAllUsers.Enabled = getAllUsers;
                    BtnGetUser.Enabled = getUser;
                    BtnCreateUser.Enabled = createUser;
                    BtnModifyUser.Enabled = modifyUser;
                    BtnGetResourcePermissions.Enabled = getResourcePermissions;
                    BtnModifyResourcePermissions.Enabled = modifyResourcePermissions;
                    BtnGetGlobalPermissions.Enabled = getGlobalPermissions;
                    BtnModifyGlobalPermissions.Enabled = modifyGlobalPermissions;
                    BtnGetResourceUsageRightsTemplate.Enabled = getResourceUsageRightsTemplate;
                    BtnModifyResourceUsageRightsTemplate.Enabled = modifyResourceUsageRightsTemplate;
                    BtnGetResource.Enabled = getResource;
                    BtnGetResourceReadOnly.Enabled = getResourceReadOnly;
                    BtnCreateNewResource.Enabled = createNewResource;
                    //BtnCreateNewResource2.Enabled = createNewResource2;
                    BtnModifyResource.Enabled = modifyResource;
                    BtnRollbackResource.Enabled = rollbackResource;
                    BtnDeleteResource.Enabled = deleteResource;
                    BtnGetVersion.Enabled = getVersion;
                    BtnGetCurrentVersion.Enabled = getCurrentVersion;
                    BtnCreateNewVersion.Enabled = createNewVersion;
                    BtnModifyVersion.Enabled = modifyVersion;
                }));
            }
            else
            {
                BtnInitialize.Enabled = initialize;
                BtnInstall.Enabled = install;
                BtnAuthenticate.Enabled = authenticate;
                BtnGetAllGroups.Enabled = getAllGroups;
                BtnGetGroup.Enabled = getGroup;
                BtnCreateGroup.Enabled = createGroup;
                BtnModifyGroup.Enabled = modifyGroup;
                BtnGetAllUsers.Enabled = getAllUsers;
                BtnGetUser.Enabled = getUser;
                BtnCreateUser.Enabled = createUser;
                BtnModifyUser.Enabled = modifyUser;
                BtnGetResourcePermissions.Enabled = getResourcePermissions;
                BtnModifyResourcePermissions.Enabled = modifyResourcePermissions;
                BtnGetGlobalPermissions.Enabled = getGlobalPermissions;
                BtnModifyGlobalPermissions.Enabled = modifyGlobalPermissions;
                BtnGetResourceUsageRightsTemplate.Enabled = getResourceUsageRightsTemplate;
                BtnModifyResourceUsageRightsTemplate.Enabled = modifyResourceUsageRightsTemplate;
                BtnGetResource.Enabled = getResource;
                BtnGetResourceReadOnly.Enabled = getResourceReadOnly;
                BtnCreateNewResource.Enabled = createNewResource;
                //BtnCreateNewResource2.Enabled = createNewResource2;
                BtnModifyResource.Enabled = modifyResource;
                BtnRollbackResource.Enabled = rollbackResource;
                BtnDeleteResource.Enabled = deleteResource;
                BtnGetVersion.Enabled = getVersion;
                BtnGetCurrentVersion.Enabled = getCurrentVersion;
                BtnCreateNewVersion.Enabled = createNewVersion;
                BtnModifyVersion.Enabled = modifyVersion;
            }
        }

        private void BtnInitialize_Click(object sender, EventArgs e)
        {
            Initialize act = new Initialize(this, _engine, _db);
            act.OnInitialized += new Initialize.InitializationDelegate(Initialize_OnInitialized);
            Clear();
            act.Test();
        }

        void Initialize_OnInitialized(bool success)
        {
            IsInitialized = true;
        }

        private void BtnGetAllGroups_Click(object sender, EventArgs e)
        {
            GetAllGroups act = new GetAllGroups(this, _engine, _db);
            act.Test();
        }

        private void BtnAuthenticate_Click(object sender, EventArgs e)
        {
            Authenticate act = new Authenticate(this, _engine, _db);
            act.OnAuthenticationSuccess += new Authenticate.AuthenticationDelegate(Authentication_OnAuthenticationSuccess);
            act.Test();
        }

        private void Authentication_OnAuthenticationSuccess(OpenDMS.Storage.Security.Session session)
        {
            Session = session;
        }

        private void BtnGetGroup_Click(object sender, EventArgs e)
        {
            GetGroup act = new GetGroup(this, _engine, _db);
            act.Test();
        }

        private void BtnCreateGroup_Click(object sender, EventArgs e)
        {
            CreateGroup act = new CreateGroup(this, _engine, _db);
            act.Test();
        }

        private void BtnModifyGroup_Click(object sender, EventArgs e)
        {
            ModifyGroup act = new ModifyGroup(this, _engine, _db);
            act.Test();
        }

        private void BtnGetAllUsers_Click(object sender, EventArgs e)
        {
            GetAllUsers act = new GetAllUsers(this, _engine, _db);
            act.Test();
        }

        private void BtnGetUser_Click(object sender, EventArgs e)
        {
            GetUser act = new GetUser(this, _engine, _db);
            act.Test();
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            CreateUser act = new CreateUser(this, _engine, _db);
            act.Test();
        }

        private void BtnModifyUser_Click(object sender, EventArgs e)
        {
            ModifyUser act = new ModifyUser(this, _engine, _db);
            act.Test();
        }

        private void BtnCreateNewResource_Click(object sender, EventArgs e)
        {
            CreateNewResource act = new CreateNewResource(this, _engine, _db);
            act.Test();
        }

        private void BtnGetResource_Click(object sender, EventArgs e)
        {
            CheckoutResource act = new CheckoutResource(this, _engine, _db);
            act.Test();
        }

        private void BtnGetResourceReadOnly_Click(object sender, EventArgs e)
        {
            GetResourceReadOnly act = new GetResourceReadOnly(this, _engine, _db);
            act.Test();
        }

        private void BtnRollbackResource_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnDeleteResource_Click(object sender, EventArgs e)
        {
            DeleteResource act = new DeleteResource(this, _engine, _db);
            act.Test();
        }

        private void BtnGetVersion_Click(object sender, EventArgs e)
        {
            CheckoutVersion act = new CheckoutVersion(this, _engine, _db);
            act.Test();
        }

        private void BtnGetCurrentVersion_Click(object sender, EventArgs e)
        {
            CheckoutCurrentVersion act = new CheckoutCurrentVersion(this, _engine, _db);
            act.Test();
        }

        private void BtnCreateNewVersion_Click(object sender, EventArgs e)
        {
            CreateNewVersion act = new CreateNewVersion(this, _engine, _db);
            act.Test();
        }

        private void BtnModifyVersion_Click(object sender, EventArgs e)
        {
            ModifyVersion act = new ModifyVersion(this, _engine, _db);
            act.Test();
        }

        private void BtnGetResourcePermissions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnModifyResourcePermissions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnGetGlobalPermissions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnModifyGlobalPermissions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnGetResourceUsageRightsTemplate_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnModifyResourceUsageRightsTemplate_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Implemented... Yet.");
        }

        private void BtnModifyResource_Click(object sender, EventArgs e)
        {
            ModifyResource act = new ModifyResource(this, _engine, _db);
            act.Test();
        }
    }
}