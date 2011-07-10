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
    public partial class FrmCreateGroup : Form
    {
        public delegate void CreateDelegate(OpenDMS.Storage.Security.Group group);
        public event CreateDelegate OnCreateClick;

        public FrmCreateGroup()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            List<string> users = TxtUsers.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            List<string> groups = TxtGroups.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            OpenDMS.Storage.Security.Group g = new OpenDMS.Storage.Security.Group(TxtGroupName.Text.Trim(), null, users, groups);
            OnCreateClick(g);
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
