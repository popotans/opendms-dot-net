using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClientTesting
{
    public partial class FrmMain : Form
    {
        public static string HOST = "127.0.0.1";
        public static int PORT = 62362;
        public static string LOG_DIRECTORY = @"C:\OpenDMS_Client\Logs\";
        public static int SendTimeout = 5000;
        public static int ReceiveTimeout = 5000;
        public static int SendBufferSize = 8192;
        public static int ReceiveBufferSize = 8192;
        private OpenDMS.ClientLibrary.Client _client;

        public FrmMain()
        {
            InitializeComponent();
            new OpenDMS.Networking.Logger(LOG_DIRECTORY);
            new OpenDMS.IO.Logger(LOG_DIRECTORY);
            _client = new OpenDMS.ClientLibrary.Client();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnPing_Click(object sender, EventArgs e)
        {
            Ping ping = new Ping(this, _client);
            ping.Test();
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

        private void btnAuthenticate_Click(object sender, EventArgs e)
        {
            Authenticate auth = new Authenticate(this, _client);
            auth.Test();
        }
    }
}
