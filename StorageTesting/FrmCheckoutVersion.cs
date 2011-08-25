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
    public partial class FrmCheckoutVersion : Form
    {
        public delegate void GoDelegate(OpenDMS.Storage.Data.VersionId versionId);
        public event GoDelegate OnGoClick;

        public FrmCheckoutVersion()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            OpenDMS.Storage.Data.VersionId versionId;

            versionId = new OpenDMS.Storage.Data.VersionId(TxtResourceId.Text.Trim() + "-" +
                TxtVersionNo.Text.Trim());

            DialogResult = System.Windows.Forms.DialogResult.OK;
            OnGoClick(versionId);
            Close();
        }
    }
}
