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
    public partial class FrmModifyUser : Form
    {
        public delegate void SaveDelegate(OpenDMS.Storage.Security.User user);
        public event SaveDelegate OnSaveClick;

        private IEngine _engine;
        private IDatabase _db;
        private OpenDMS.Storage.Security.Session _session;
        private List<OpenDMS.Storage.Security.User> _users;

        public FrmModifyUser(IEngine engine, IDatabase db, OpenDMS.Storage.Security.Session session)
        {
            InitializeComponent();
            _engine = engine;
            _db = db;
            _session = session;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            List<string> groups = TxtGroups.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            if (TxtPassword.Text.Trim() == "" || TxtPassword.Text.Trim() == "<encrypted>")
            {
                MessageBox.Show("You must enter a password to be saved.");
                return;
            }

            OpenDMS.Storage.Security.User user = (OpenDMS.Storage.Security.User)comboBox1.SelectedItem;
            OpenDMS.Storage.Security.User u = new OpenDMS.Storage.Security.User(user.Id,
                user.Rev, TxtPassword.Text.Trim(), TxtFirstName.Text.Trim(), TxtMiddleName.Text.Trim(),
                TxtLastName.Text.Trim(), groups, checkBox1.Checked);
            OnSaveClick(u);
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void FrmModifyUser_Load(object sender, EventArgs e)
        {
            panel1.Visible = true;

            EngineRequest request = new EngineRequest();
            request.Engine = _engine;
            request.Database = _db;
            request.OnComplete += new EngineBase.CompletionDelegate(Complete);
            request.OnTimeout += new EngineBase.TimeoutDelegate(Timeout);
            request.OnError += new EngineBase.ErrorDelegate(Error);
            request.AuthToken = _session.AuthToken;
            request.RequestingPartyType = OpenDMS.Storage.Security.RequestingPartyType.System;

            _engine.GetAllUsers(request);
        }

        private void Complete(EngineRequest request, ICommandReply reply)
        {
            OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply r = (OpenDMS.Storage.Providers.CouchDB.Commands.GetViewReply)reply;

            OpenDMS.Storage.Providers.CouchDB.Transitions.UserCollection uc = new OpenDMS.Storage.Providers.CouchDB.Transitions.UserCollection();
            List<OpenDMS.Storage.Security.User> users = uc.Transition(r.View);

            for (int i = 0; i < users.Count; i++)
            {
                comboBox1.Invoke(new MethodInvoker(delegate { comboBox1.Items.Add(users[i]); }));
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
            OpenDMS.Storage.Security.User user = null;
            TxtGroups.Invoke(new MethodInvoker(delegate 
                {
                    user = (OpenDMS.Storage.Security.User)comboBox1.SelectedItem;
                    TxtFirstName.Text = user.FirstName;
                    TxtMiddleName.Text = user.MiddleName;
                    TxtLastName.Text = user.LastName;
                    TxtPassword.Text = "<encrypted>";
                    TxtGroups.Text = "";
                }));
            
            if (user.Groups != null)
                for (int i = 0; i < user.Groups.Count; i++)
                    TxtGroups.Invoke(new MethodInvoker(delegate { TxtGroups.Text += user.Groups[i] + "\r\n"; }));
        }
    }
}
