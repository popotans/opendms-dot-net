using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowsClient
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        void textBox1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog win = new System.Windows.Forms.FolderBrowserDialog();
            win.RootFolder = Environment.SpecialFolder.MyComputer;
            win.Description = "Select the location that LawOffice File Manager should use for storage";
            win.ShowNewFolderButton = true;
            switch (win.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.OK:
                    textBox1.Text = win.SelectedPath;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                    break;
                default:
                    break;
            }
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Text = MainWindow.Settings.StorageLocation;
            textBox2.Text = Common.ServerSettings.Instance.ServerIp;
            textBox3.Text = Common.ServerSettings.Instance.ServerPort.ToString();
        }

        void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.StorageLocation = textBox1.Text.Trim();
            Common.ServerSettings.Instance.ServerIp = textBox2.Text.Trim();
            Common.ServerSettings.Instance.ServerPort = int.Parse(textBox3.Text.Trim());
            MainWindow.Settings.Save();
            this.Close();
        }
    }
}
