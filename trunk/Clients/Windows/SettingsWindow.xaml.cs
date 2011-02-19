/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the PreviewMouseUp event of the textBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Text = Settings.Instance.StorageLocation;
            textBox2.Text = Settings.Instance.ServerIp;
            textBox3.Text = Settings.Instance.ServerPort.ToString();
        }

        /// <summary>
        /// Handles the Click event of the BtnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.StorageLocation = textBox1.Text.Trim();
            Settings.Instance.ServerIp = textBox2.Text.Trim();
            Settings.Instance.ServerPort = int.Parse(textBox3.Text.Trim());
            Settings.Instance.Save(Utilities.GetAppDataPath() + "Settings.xml");
            this.Close();
        }
    }
}
