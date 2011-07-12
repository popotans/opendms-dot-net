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
    public partial class FrmGetUser : Form
    {
        public delegate void GoDelegate(string userName);
        public event GoDelegate OnGoClick;

        public FrmGetUser()
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
