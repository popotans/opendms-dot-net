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
    public partial class FrmGetGroup : Form
    {
        public delegate void GoDelegate(string groupName);
        public event GoDelegate OnGoClick;

        public FrmGetGroup()
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
            DialogResult = System.Windows.Forms.DialogResult.OK;
            OnGoClick(textBox1.Text.Trim());
            Close();
        }
    }
}
