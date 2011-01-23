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
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace WindowsClient
{
    /// <summary>
    /// Interaction logic for MetaPropWindow.xaml
    /// </summary>
    public partial class MetaPropWindow : Window
    {
        private Guid _guid;
        private delegate void UpdateUI();
        private Common.Data.MetaAsset _metaasset;

        public MetaPropWindow(Guid guid)
        {
            InitializeComponent();
            _guid = guid;
            _metaasset = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _metaasset = new Common.Data.MetaAsset(_guid, MainWindow.FileSystem, MainWindow.GeneralLogger);
            _metaasset.Load(MainWindow.GeneralLogger);
            TxtTitle.Text = _metaasset.Title;
            TxtTags.Text = "";

            for (int i = 0; i < _metaasset.Tags.Count; i++)
                TxtTags.Text += _metaasset.Tags[i] + ", ";

            TxtTags.Text = TxtTags.Text.Substring(0, TxtTags.Text.Length - 2);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _metaasset.UpdateByUser(TxtTitle.Text.Trim(), ParseTags());
            _metaasset.Save();
            DialogResult = true;
            this.Close();
        }

        private List<string> ParseTags()
        {
            List<string> tags = new List<string>();
            string[] ret = TxtTags.Text.Split(',');

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = ret[i].Trim();
                if (ret[i].Length > 0)
                    tags.Add(ret[i]);
            }

            return tags;
        }
    }
}
