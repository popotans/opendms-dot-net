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
    public partial class FrmModifyGroup : Form
    {
        public delegate void SaveDelegate(OpenDMS.Storage.Security.Group group);
        public event SaveDelegate OnSaveClick;

        private IEngine _engine;
        private IDatabase _db;
        private OpenDMS.Storage.Security.Session _session;
        private List<OpenDMS.Storage.Security.Group> _groups;

        public FrmModifyGroup(IEngine engine, IDatabase db, OpenDMS.Storage.Security.Session session)
        {
            InitializeComponent();
            _engine = engine;
            _db = db;
            _session = session;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            List<string> users = TxtUsers.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            List<string> groups = TxtGroups.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            OpenDMS.Storage.Security.Group group = (OpenDMS.Storage.Security.Group)comboBox1.SelectedItem;
            OpenDMS.Storage.Security.Group g = new OpenDMS.Storage.Security.Group(group.Id, group.Rev, users, groups);
            OnSaveClick(g);
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void FrmModifyGroup_Load(object sender, EventArgs e)
        {
            panel1.Visible = true;

            EngineRequest request = new EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            //request.OnActionChanged += new EngineBase.ActionDelegate(EngineAction);
            //request.OnProgress += new EngineBase.ProgressDelegate(Progress);
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.AuthToken = _session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            _engine.GetAllGroups(request);
        }

        private void Progress(EngineRequest request, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply)reply;

            OpenDMS.Storage.Providers.CouchDB.Transitions.GroupCollection gc = new OpenDMS.Storage.Providers.CouchDB.Transitions.GroupCollection();
            List<OpenDMS.Storage.Security.Group> groups = gc.Transition(r.View);

            for (int i = 0; i < groups.Count; i++)
            {
                comboBox1.Invoke(new MethodInvoker(delegate { comboBox1.Items.Add(groups[i]); }));
            }

            comboBox1.Invoke(new MethodInvoker(delegate { panel1.Visible = false; }));
        }

        private void Timeout(EngineRequest request)
        {
        }

        private void Error(EngineRequest request, string message, Exception exception)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OpenDMS.Storage.Security.Group group = null;
            TxtGroups.Invoke(new MethodInvoker(delegate { group = (OpenDMS.Storage.Security.Group)comboBox1.SelectedItem; }));

            TxtGroups.Invoke(new MethodInvoker(delegate { TxtGroups.Text = ""; }));
            TxtUsers.Invoke(new MethodInvoker(delegate { TxtUsers.Text = ""; }));

            if (group.Groups != null)
                for (int i = 0; i < group.Groups.Count; i++)
                    TxtGroups.Invoke(new MethodInvoker(delegate { TxtGroups.Text += group.Groups[i] + "\r\n"; }));

            if (group.Users != null)
                for (int i = 0; i < group.Users.Count; i++)
                    TxtUsers.Invoke(new MethodInvoker(delegate { TxtUsers.Text += group.Users[i] + "\r\n"; }));
        }
    }
}
