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
        private Common.NetworkPackage.MetaForm _metaForm;
        private TreeViewItem _selectedTvi;
        //private System.Collections.Generic.Dictionary<string, object> _propertyUpdates;

        public MetaPropWindow(Guid guid)
        {
            InitializeComponent();
            _metaForm = null;
            _guid = guid;
            _metaasset = null;
            _selectedTvi = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread download;

            _metaasset = new Common.Data.MetaAsset(_guid, MainWindow.FileSystem, MainWindow.GeneralLogger);
            if (!_metaasset.Load(MainWindow.GeneralLogger))
            {
                MessageBox.Show("The resource's meta data could not be loaded.");
                return;
            }
            //LoadTree(_metaasset);

            download = new Thread(new ThreadStart(DownloadFormProperties));
            download.Start();
        }

        private void DownloadFormProperties()
        {
            Common.Network.Message msg = null;
            UpdateUI actUpdateUI = UpdateUI_DownloadComplete;

            try
            {
                msg = new Common.Network.Message(Common.ServerSettings.Instance.ServerIp, Common.ServerSettings.Instance.ServerPort, "_settings", "metaform", Common.Network.OperationType.GET,
                     Common.Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true, Common.ServerSettings.Instance.NetworkBufferSize,
                     Common.ServerSettings.Instance.NetworkTimeout, MainWindow.GeneralLogger, MainWindow.NetworkLogger);
                msg.Send();
            }
            catch (Exception e)
            {
                MainWindow.ErrorManager.AddError(Common.ErrorMessage.MetaFormDownloadFailed(e));
                return;
            }

            _metaForm = new Common.NetworkPackage.MetaForm();

            try
            {
                _metaForm.Deserialize(msg.State.Stream);
            }
            catch (Exception e)
            {
                MainWindow.ErrorManager.AddError(Common.ErrorMessage.MetaFormDeserializationFailed(e));
                return;
            }

            Dispatcher.BeginInvoke(actUpdateUI, System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void UpdateUI_DownloadComplete()
        {
            //Storyboard sbBegin = (Storyboard)TryFindResource("BeginLoadingSearchOptions");
            //Storyboard sbEnd = (Storyboard)TryFindResource("EndLoadingSearchOptions");

            //if (sbBegin.GetCurrentState() != ClockState.Stopped)
            //    sbBegin.Stop();

            //sbEnd.Begin();
            LoadTree();
        }

        private void LoadTree()
        {
            object tempObj;
            Common.NetworkPackage.MetaAsset netMa;
            Dictionary<string, object>.Enumerator en;

            // All of our *required* properties are going to be stated in _metaForm
            // All of our *current* properties are going to be in _metaasset
            // _metaasset can have properties that _metaForm does not if and only if 
            //      those properties exist as "UserProperties".
            // _metaasset must have all the properties that _metaForm has


            // 1) Build tree based on _metaForm using its defaults
            // 2) Replace default values with values in _metaasset
            // 3) Add to tree based on _metaasset.UserProperties

            netMa = _metaasset.ExportToNetworkRepresentation();
            en = _metaasset.UserProperties.GetEnumerator();

            // Step 1 and 2
            for (int i = 0; i < _metaForm.Count; i++)
            {
                // Here we accomplish 1 and 2 above at the same time (more efficient)
                if((tempObj = netMa[_metaForm[i].PropertyName]) == null)
                    AddTreeViewItem(_metaForm[i].PropertyName, _metaForm[i].Label, _metaForm[i].DefaultValue, _metaForm[i].IsReadOnly);
                else
                    AddTreeViewItem(_metaForm[i].PropertyName, _metaForm[i].Label, tempObj, _metaForm[i].IsReadOnly);
            }

            // Step 3
            while (en.MoveNext())
            {
                AddTreeViewItem(en.Current.Key, en.Current.Key, en.Current.Value, false);
            }
        }
        
        private void AddTreeViewItem(string key, string title, object obj, bool isReadOnly)
        {
            TreeViewItem tvi = new TreeViewItem();
            MetaPropEntity mpe;

            mpe = new MetaPropEntity(key, title, obj, isReadOnly);

            tvi.Header = title;
            tvi.Tag = mpe;
            tvi.Selected += new RoutedEventHandler(TreeViewItem_Selected);

            if (isReadOnly)
                tvi.Background = new SolidColorBrush(Color.FromArgb(75, 255, 0, 0)); // Red

            treeView1.Items.Add(tvi);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Saves
            if (_selectedTvi != null && !((MetaPropEntity)_selectedTvi.Tag).IsReadOnly)
                SaveChangeBackToTvi(_selectedTvi);

            UpdateMetaAssetFromTree();
            DialogResult = true;
            this.Close();
        }

        private void UpdateMetaAssetFromTree()
        {
            Common.NetworkPackage.MetaAsset netMa;
            MetaPropEntity mpe;

            netMa = _metaasset.ExportToNetworkRepresentation();

            for (int i = 0; i < treeView1.Items.Count; i++)
            {
                mpe = (MetaPropEntity)((TreeViewItem)treeView1.Items[i]).Tag;
                if (mpe.IsUpdated && !mpe.IsReadOnly)
                {
                    if (netMa.ContainsKey(mpe.Key))
                        netMa[mpe.Key] = mpe.Value;
                    else
                        netMa.Add(mpe.Key, mpe.Value);
                }
            }

            // Common.Data.MetaAsset requires that the '$tags' property be of type string[], so if it is not
            // then lets make it.
            if (netMa["$tags"].GetType() == typeof(List<string>))
                netMa["$tags"] = ((List<string>)netMa["$tags"]).ToArray();

            _metaasset.ImportFromNetworkRepresentation(netMa);
            _metaasset.Save();
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            // Saves
            if (_selectedTvi != null && !((MetaPropEntity)_selectedTvi.Tag).IsReadOnly)
                SaveChangeBackToTvi(_selectedTvi);

            // New
            _selectedTvi = (TreeViewItem)sender;

            BuildGui((MetaPropEntity)_selectedTvi.Tag);
        }

        private bool SaveChangeBackToTvi(TreeViewItem destinationTvi)
        {
            MetaPropEntity mpe = (MetaPropEntity)destinationTvi.Tag;

            if (mpe.Value.GetType() == typeof(string))
            {
                mpe.Value = GetTextBoxValue();
                mpe.IsUpdated = true;
            }
            else if (mpe.Value.GetType() == typeof(Guid))
            {
                Guid var_guid;

                if (Guid.TryParse(GetTextBoxValue(), out var_guid))
                {
                    mpe.Value = var_guid;
                    mpe.IsUpdated = true;
                }
                else
                {
                    MessageBox.Show("The system cannot save the Guid as it appears to be invalid.", "Invalid Guid");
                    return false;
                }
            }
            else if (mpe.Value.GetType() == typeof(Int32))
            {
                Int32 var_int32;

                if (Int32.TryParse(GetTextBoxValue(), out var_int32))
                {
                    mpe.Value = var_int32;
                    mpe.IsUpdated = true;
                }
                else
                {
                    MessageBox.Show("The system cannot save the value as it appears to be invalid.  The value must be numeric between " + Int32.MinValue.ToString() + " and " + Int32.MaxValue.ToString() + ".", "Invalid Value");
                    return false;
                }
            }
            else if (mpe.Value.GetType() == typeof(UInt32))
            {
                UInt32 var_uint32;

                if (UInt32.TryParse(GetTextBoxValue(), out var_uint32))
                {
                    mpe.Value = var_uint32;
                    mpe.IsUpdated = true;
                }
                else
                {
                    MessageBox.Show("The system cannot save the value as it appears to be invalid.  The value must be numeric between " + UInt32.MinValue.ToString() + " and " + UInt32.MaxValue.ToString() + ".", "Invalid Value");
                    return false;
                }
            }
            else if (mpe.Value.GetType() == typeof(Int64))
            {
                Int64 var_int64;

                if (Int64.TryParse(GetTextBoxValue(), out var_int64))
                {
                    mpe.Value = var_int64;
                    mpe.IsUpdated = true;
                }
                else
                {
                    MessageBox.Show("The system cannot save the value as it appears to be invalid.  The value must be numeric between " + Int64.MinValue.ToString() + " and " + Int64.MaxValue.ToString() + ".", "Invalid Value");
                    return false;
                }
            }
            else if (mpe.Value.GetType() == typeof(UInt64))
            {
                UInt64 var_uint64;

                if (UInt64.TryParse(GetTextBoxValue(), out var_uint64))
                {
                    mpe.Value = var_uint64;
                    mpe.IsUpdated = true;
                }
                else
                {
                    MessageBox.Show("The system cannot save the value as it appears to be invalid.  The value must be numeric between " + UInt64.MinValue.ToString() + " and " + UInt64.MaxValue.ToString() + ".", "Invalid Value");
                    return false;
                }
            }
            else if (mpe.Value.GetType() == typeof(DateTime))
            {
                DateTime? var_dt = GetDateTimeValue();

                if (!var_dt.HasValue)
                {
                    MessageBox.Show("You must select a date.", "Invalid Date");
                    return false;
                }

                mpe.Value = var_dt;
                mpe.IsUpdated = true;
            }
            else if (mpe.Value.GetType() == typeof(bool))
            {
                bool? var_bool = GetCheckBoxValue();

                if (!var_bool.HasValue)
                {
                    MessageBox.Show("A checkbox was expected but could not be located", "Invalid CheckBox");
                    return false;
                }

                mpe.Value = var_bool;
                mpe.IsUpdated = true;
            }
            else if (mpe.Value.GetType() == typeof(System.Collections.Generic.List<string>))
            {
                System.Collections.Generic.List<string> var_list = ParseListFromString(GetTextBoxValue());

                if (var_list.Count <= 0)
                {
                    if (MessageBox.Show("No tags were entered, it is going to be really hard to locate this resource if you do not specify some tags.\r\nWould you like to add tags?", "Tags Desired?",
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        return false; // this prevents the UI from changing
                }

                mpe.Value = var_list.ToArray();
                mpe.IsUpdated = true;
            }
            else if (mpe.Value.GetType() == typeof(Common.Data.ETag))
            {
                throw new Exception("Invalid, a user can never edit an ETag");
            }

            return true;
        }

        private string GetTextBoxValue()
        {
            StackPanel sp = (StackPanel)UIPanel.Children[0];
            for (int i = 0; i < sp.Children.Count; i++)
            {
                if (sp.Children[i].GetType() == typeof(TextBox))
                {
                    return ((TextBox)sp.Children[i]).Text;
                }
            }

            return null;
        }

        private DateTime? GetDateTimeValue()
        {
            string output = "";

            StackPanel sp = (StackPanel)UIPanel.Children[0];
            for (int i = 0; i < sp.Children.Count; i++)
            {
                if (sp.Children[i].GetType() == typeof(DatePicker))
                {
                    output = ((DatePicker)sp.Children[i]).SelectedDate.Value.ToShortDateString();
                }
                else if (sp.Children[i].GetType() == typeof(TextBox))
                {
                    DateTime dt;

                    output += " " + ((TextBox)sp.Children[i]).Text.Trim();

                    if (!DateTime.TryParse(output, out dt))
                    {
                        MessageBox.Show("The time entered has an invalid format, please enter the time using the same format in which it loaded.", "Invalid Time Format");
                        return null;
                    }

                    return dt;
                }
            }

            return null;
        }

        private bool? GetCheckBoxValue()
        {
            StackPanel sp = (StackPanel)UIPanel.Children[0];
            for (int i = 0; i < sp.Children.Count; i++)
            {
                if (sp.Children[i].GetType() == typeof(CheckBox))
                {
                    return ((CheckBox)sp.Children[i]).IsChecked.Value;
                }
            }

            return null;
        }

        private System.Collections.Generic.List<string> ParseListFromString(string input)
        {
            string[] temp;
            System.Collections.Generic.List<string> output = new List<string>();

            temp = input.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i].Trim().Length > 0)
                    output.Add(temp[i]);
            }

            return output;
        }

        private void BuildGui(MetaPropEntity mpe)
        {
            UIPanel.Children.Clear();

            if (mpe.Value.GetType() == typeof(string))
            {
                UIPanel.Children.Add(AddText(mpe.Title, (string)mpe.Value, mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(Guid))
            {
                UIPanel.Children.Add(AddText(mpe.Title, ((Guid)mpe.Value).ToString("N"), mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(Int32))
            {
                UIPanel.Children.Add(AddText(mpe.Title, ((Int32)mpe.Value).ToString(), mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(UInt32))
            {
                UIPanel.Children.Add(AddText(mpe.Title, ((UInt32)mpe.Value).ToString(), mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(Int64))
            {
                UIPanel.Children.Add(AddText(mpe.Title, ((Int64)mpe.Value).ToString(), mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(UInt64))
            {
                UIPanel.Children.Add(AddText(mpe.Title, ((UInt64)mpe.Value).ToString(), mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(DateTime))
            {
                UIPanel.Children.Add(AddDateTime(mpe.Title, (DateTime)mpe.Value, mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(bool))
            {
                UIPanel.Children.Add(AddCheckBox(mpe.Title, (bool)mpe.Value, mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(List<string>))
            {
                UIPanel.Children.Add(AddMultilineText(mpe.Title, string.Join("\r\n", ((List<string>)mpe.Value).ToArray()), mpe.IsReadOnly));
            }
            else if (mpe.Value.GetType() == typeof(Common.Data.ETag))
            {
                UIPanel.Children.Add(AddText(mpe.Title, ((Common.Data.ETag)mpe.Value).Value, mpe.IsReadOnly));
            }
            else
            {
                Type type = mpe.Value.GetType();
                throw new Exception("Unknown type '" + type.FullName + "' encountered.");
            }
        }

        private StackPanel AddText(string title, string value, bool isReadOnly)
        {
            StackPanel sp = new StackPanel();
            Label lblTitle = new Label();
            TextBox txtValue = new TextBox();

            sp.Orientation = Orientation.Vertical;

            lblTitle.Margin = new Thickness(10, 0, 10, 0);

            if (isReadOnly)
                lblTitle.Content = title + " (read-only):";
            else
                lblTitle.Content = title + ":";

            txtValue.Margin = new Thickness(10, 0, 10, 0);
            txtValue.Text = value;
            txtValue.IsReadOnly = isReadOnly;
            txtValue.IsEnabled = !isReadOnly;

            sp.Children.Add(lblTitle);
            sp.Children.Add(txtValue);

            return sp;
        }

        private StackPanel AddDateTime(string title, DateTime value, bool isReadOnly)
        {
            StackPanel sp = new StackPanel();
            Label lblTitle1 = new Label();
            DatePicker datePicker = new DatePicker();
            Label lblTitle2 = new Label();
            TextBox txtTime = new TextBox();

            sp.Orientation = Orientation.Vertical;

            lblTitle1.Margin = new Thickness(10, 0, 10, 0);

            if (isReadOnly)
                lblTitle1.Content = title + " (read-only):";
            else
                lblTitle1.Content = title + ":";

            datePicker.Margin = new Thickness(10, 0, 10, 0);
            datePicker.IsEnabled = !isReadOnly;
            datePicker.SelectedDate = value;

            lblTitle2.Margin = new Thickness(10, 0, 10, 0);
            lblTitle2.Content = "Enter the Time:";

            txtTime.Margin = new Thickness(10, 0, 10, 0);
            txtTime.IsEnabled = !isReadOnly;
            txtTime.IsReadOnly = isReadOnly;
            txtTime.Text = value.ToLongTimeString();

            sp.Children.Add(lblTitle1);
            sp.Children.Add(datePicker);
            sp.Children.Add(lblTitle2);
            sp.Children.Add(txtTime);

            return sp;
        }

        private StackPanel AddCheckBox(string title, bool isChecked, bool isReadOnly)
        {
            StackPanel sp = new StackPanel();
            Label lblTitle = new Label();
            CheckBox checkBox = new CheckBox();

            sp.Orientation = Orientation.Vertical;

            lblTitle.Margin = new Thickness(10, 0, 10, 0);

            if (isReadOnly)
                lblTitle.Content = title + " (read-only):";
            else
                lblTitle.Content = title + ":";

            checkBox.Margin = new Thickness(10, 0, 10, 0);
            checkBox.IsEnabled = !isReadOnly;
            checkBox.IsChecked = isChecked;

            sp.Children.Add(lblTitle);
            sp.Children.Add(checkBox);

            return sp;
        }

        private StackPanel AddMultilineText(string title, string value, bool isReadOnly)
        {
            StackPanel sp = new StackPanel();
            Label lblTitle = new Label();
            TextBox txtValue = new TextBox();

            sp.Orientation = Orientation.Vertical;

            lblTitle.Margin = new Thickness(10, 0, 10, 0);

            if (isReadOnly)
                lblTitle.Content = title + " (read-only):";
            else
                lblTitle.Content = title + ":";

            txtValue.Margin = new Thickness(10, 0, 10, 0);
            txtValue.Text = value;
            txtValue.IsReadOnly = isReadOnly;
            txtValue.IsEnabled = !isReadOnly;
            txtValue.TextWrapping = TextWrapping.Wrap;
            txtValue.AcceptsReturn = true;
            txtValue.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            txtValue.Height = 350;

            sp.Children.Add(lblTitle);
            sp.Children.Add(txtValue);

            return sp;
        }
    }
}
