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
    public partial class FrmLogin : Form
    {
        public delegate void CancelDelegate();
        public delegate void LoginDelegate(string username, string password);
        public event CancelDelegate OnCancelClick;
        public event LoginDelegate OnLoginClick;

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            OnCancelClick();
            Close();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            OnLoginClick(textBox1.Text.Trim(), maskedTextBox1.Text.Trim());
            Close();
        }
    }
}
