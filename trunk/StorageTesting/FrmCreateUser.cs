using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StorageTesting
{
    public partial class FrmCreateUser : Form
    {
        public delegate void CreateDelegate(OpenDMS.Storage.Security.User user);
        public event CreateDelegate OnCreateClick;

        public FrmCreateUser()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            List<string> groups = TxtGroups.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            OpenDMS.Storage.Security.User user = new OpenDMS.Storage.Security.User(
                TxtUserName.Text.Trim(), null, TxtPassword.Text.Trim(), TxtFirstName.Text.Trim(),
                TxtMiddleName.Text.Trim(), TxtLastName.Text.Trim(), groups, checkBox1.Checked);
            OnCreateClick(user);
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
