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
        /// <summary>
        /// The <see cref="Guid"/> of the Asset being displayed.
        /// </summary>
        private Guid _guid;
        /// <summary>
        /// Reference to the method that handles the update ui events.
        /// </summary>
        private delegate void UpdateUI();
        /// <summary>
        /// The <see cref="Common.Data.MetaAsset"/> being displayed.
        /// </summary>
        private Common.Data.MetaAsset _metaasset;
        /// <summary>
        /// The <see cref="Common.NetworkPackage.MetaForm"/> being displayed.
        /// </summary>
        private Common.NetworkPackage.MetaForm _metaForm;
        /// <summary>
        /// The currently selected <see cref="TreeViewItem"/>
        /// </summary>
        private TreeViewItem _selectedTvi;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaPropWindow"/> class.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> of the Asset.</param>
        public MetaPropWindow(Guid guid)
        {
            InitializeComponent();
            _metaForm = null;
            _guid = guid;
            _metaasset = null;
            _selectedTvi = null;
        }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread download;

            _metaasset = new Common.Data.MetaAsset(_guid, MainWindow.FileSystem);
            if (!_metaasset.Load())
            {
                MessageBox.Show("The resource's meta data could not be loaded.");
                return;
            }
            //LoadTree(_metaasset);

            download = new Thread(new ThreadStart(DownloadFormProperties));
            download.Start();
        }

        /// <summary>
        /// Downloads the <see cref="Common.NetworkPackage.MetaForm"/> which contains the collection of properties 
        /// for be displayed on the UI
        /// </summary>
        private void DownloadFormProperties()
        {
            Common.Network.Message msg = null;
            UpdateUI actUpdateUI = UpdateUI_DownloadComplete;

            try
            {
                msg = new Common.Network.Message(Settings.Instance.ServerIp, Settings.Instance.ServerPort, "_settings", "metaform", Common.Network.OperationType.GET,
                     Common.Network.DataStreamMethod.Memory, null, null, null, null, false, false, true, true, Settings.Instance.NetworkBufferSize,
                     Settings.Instance.NetworkTimeout);
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

        /// <summary>
        /// Called when the <see cref="Common.NetworkPackage.MetaForm"/> download is complete.
        /// </summary>
        private void UpdateUI_DownloadComplete()
        {
            //Storyboard sbBegin = (Storyboard)TryFindResource("BeginLoadingSearchOptions");
            //Storyboard sbEnd = (Storyboard)TryFindResource("EndLoadingSearchOptions");

            //if (sbBegin.GetCurrentState() != ClockState.Stopped)
            //    sbBegin.Stop();

            //sbEnd.Begin();
            LoadTree();
        }

        /// <summary>
        /// Loads the meta properties into the UI tree.
        /// </summary>
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

        /// <summary>
        /// Creates a <see cref="MetaPropEntity"/> and adds it to the UI tree.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="title">The title.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="isReadOnly">If set to <c>true</c> the property is read-only and the user cannot change it; 
        /// otherwise, <c>false</c> allows user modification.</param>
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

        /// <summary>
        /// Handles the Click event of the BtnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of the BtnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Saves
            if (_selectedTvi != null && !((MetaPropEntity)_selectedTvi.Tag).IsReadOnly)
                SaveChangeBackToTvi(_selectedTvi);

            UpdateMetaAssetFromTree();
            DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Updates the <see cref="Common.Data.MetaAsset"/> from UI tree.
        /// </summary>
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

        /// <summary>
        /// Handles the Selected event of the TreeViewItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            // Saves if needed
            if (_selectedTvi != null && !((MetaPropEntity)_selectedTvi.Tag).IsReadOnly)
                SaveChangeBackToTvi(_selectedTvi);

            // New
            _selectedTvi = (TreeViewItem)sender;

            BuildGui((MetaPropEntity)_selectedTvi.Tag);
        }

        /// <summary>
        /// Saves any change to the Tag property of the destinationTvi.
        /// </summary>
        /// <param name="destinationTvi">The destination tvi.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.  In the event of a false return 
        /// the changes have not been saved and the user will be notified.</returns>
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

                mpe.Value = var_list;
                mpe.IsUpdated = true;
            }
            else if (mpe.Value.GetType() == typeof(Common.Data.ETag))
            {
                throw new Exception("Invalid, a user can never edit an ETag");
            }

            return true;
        }

        /// <summary>
        /// Locates the <see cref="TextBox"/> for data input and gets a string value representing the data contained therein.
        /// </summary>
        /// <returns>The text value of the <see cref="TextBox"/>.</returns>
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

        /// <summary>
        /// Locates the <see cref="DatePicker"/> for data input and the <see cref="TextBox"/> and gets the date and time therefrom, respectively.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> if no error is found in parsing; otherwise, <c>null</c> if an error is encountered.</returns>
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

        /// <summary>
        /// Locates the <see cref="CheckBox"/> for data input and gets a string value representing the data contained therein.
        /// </summary>
        /// <returns>The checked value of the <see cref="CheckBox"/>.</returns>
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

        /// <summary>
        /// Parses a <see cref="T:System.Collections.Generic.List&lt;string&gt;"/> from a string seperating on "\r\n".
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>A list of strings.</returns>
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

        /// <summary>
        /// Builds the right-hand side of the GUI based on the argument.
        /// </summary>
        /// <param name="mpe">The <see cref="MetaPropEntity"/>.</param>
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

        /// <summary>
        /// Adds controls suitable for textual input.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="value">The value.</param>
        /// <param name="isReadOnly">if set to <c>true</c> then the control is 
        /// read-only and cannot be modified by the user.</param>
        /// <returns>A <see cref="StackPanel"/> containing all the necessary UI controls.</returns>
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

        /// <summary>
        /// Adds controls suitable for date and time input.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="value">The value.</param>
        /// <param name="isReadOnly">if set to <c>true</c> then the control is 
        /// read-only and cannot be modified by the user.</param>
        /// <returns>A <see cref="StackPanel"/> containing all the necessary UI controls.</returns>
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

        /// <summary>
        /// Adds controls suitable for check box input.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="isChecked">if set to <c>true</c> the box is checked.</param>
        /// <param name="isReadOnly">if set to <c>true</c> then the control is 
        /// read-only and cannot be modified by the user.</param>
        /// <returns>A <see cref="StackPanel"/> containing all the necessary UI controls.</returns>
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

        /// <summary>
        /// Adds controls suitable for multiline textual input.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="value">The value.</param>
        /// <param name="isReadOnly">if set to <c>true</c> then the control is 
        /// read-only and cannot be modified by the user.</param>
        /// <returns>A <see cref="StackPanel"/> containing all the necessary UI controls.</returns>
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
