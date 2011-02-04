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
using System.Windows;

namespace WindowsClient
{
    /// <summary>
    /// Tracks properties of a <see cref="System.Windows.Controls.TreeViewItem"/>.
    /// </summary>
    public static class TreeViewItemProps
    {
        /// <summary>
        /// Gets the IsLoading property.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <returns>The value of the property</returns>
        public static bool GetIsLoading(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLoadingProperty);
        }

        /// <summary>
        /// Sets the IsLoading property.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsLoading(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLoadingProperty, value);
        }

        /// <summary>
        /// Gets the IsCanceled property.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <returns>The value of the property</returns>
        public static bool GetIsCanceled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCanceledProperty);
        }

        /// <summary>
        /// Sets the IsCanceled property.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <param name="value">The value.</param>
        public static void SetIsCanceled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCanceledProperty, value);
        }

        /// <summary>
        /// Gets the percent complete.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <returns>The value of the property</returns>
        public static int GetPercentComplete(DependencyObject obj)
        {
            return (int)obj.GetValue(PercentCompleteProperty);
        }

        /// <summary>
        /// Sets the percent complete.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <param name="value">The value.</param>
        public static void SetPercentComplete(DependencyObject obj, int value)
        {
            obj.SetValue(PercentCompleteProperty, value);
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <returns>The value of the property</returns>
        public static string GetStatus(DependencyObject obj)
        {
            return (string)obj.GetValue(StatusProperty);
        }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <param name="value">The value.</param>
        public static void SetStatus(DependencyObject obj, string value)
        {
            obj.SetValue(StatusProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="Guid"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <returns>The value of the property</returns>
        public static string GetGuid(DependencyObject obj)
        {
            return (string)obj.GetValue(GuidProperty);
        }

        /// <summary>
        /// Sets the <see cref="Guid"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Windows.Controls.TreeViewItem"/>.</param>
        /// <param name="value">The value.</param>
        public static void SetGuid(DependencyObject obj, string value)
        {
            obj.SetValue(GuidProperty, value);
        }

        /// <summary>
        /// The property tracking if the asset is loading.
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty; //
        /// <summary>
        /// The property tracking if the action is canceled.
        /// </summary>
        public static readonly DependencyProperty IsCanceledProperty; //
        /// <summary>
        /// The percent complete.
        /// </summary>
        public static readonly DependencyProperty PercentCompleteProperty; //
        /// <summary>
        /// The status.
        /// </summary>
        public static readonly DependencyProperty StatusProperty;
        /// <summary>
        /// The <see cref="Guid"/>.
        /// </summary>
        public static readonly DependencyProperty GuidProperty;




        /// <summary>
        /// Initializes the <see cref="TreeViewItemProps"/> class.
        /// </summary>
        static TreeViewItemProps()
        {
            IsLoadingProperty = DependencyProperty.RegisterAttached("IsLoading", typeof(bool), typeof(TreeViewItemProps),
                                                                    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

            //IsLoadedProperty = DependencyProperty.RegisterAttached("IsLoaded", typeof(bool), typeof(TreeViewItemProps),
            //                                                       new FrameworkPropertyMetadata(false));

            IsCanceledProperty = DependencyProperty.RegisterAttached("IsCanceled", typeof(bool), typeof(TreeViewItemProps),
                                                                     new FrameworkPropertyMetadata(false));

            PercentCompleteProperty = DependencyProperty.RegisterAttached("PercentComplete", typeof(int), typeof(TreeViewItemProps),
                                                                          new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

            StatusProperty = DependencyProperty.RegisterAttached("Status", typeof(string), typeof(TreeViewItemProps),
                                                                 new FrameworkPropertyMetadata(null));

            GuidProperty = DependencyProperty.RegisterAttached("Guid", typeof(string), typeof(TreeViewItemProps),
                                                               new FrameworkPropertyMetadata(null));
        }
    }
}
