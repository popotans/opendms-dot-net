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
    public static class TreeViewItemProps
    {
        public static bool GetIsLoading(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLoadingProperty);
        }

        public static void SetIsLoading(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLoadingProperty, value);
        }

        //public static bool GetIsLoaded(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(IsLoadedProperty);
        //}

        //public static void SetIsLoaded(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(IsLoadedProperty, value);
        //}

        public static bool GetIsCanceled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCanceledProperty);
        }

        public static void SetIsCanceled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCanceledProperty, value);
        }

        public static int GetPercentComplete(DependencyObject obj)
        {
            return (int)obj.GetValue(PercentCompleteProperty);
        }

        public static void SetPercentComplete(DependencyObject obj, int value)
        {
            obj.SetValue(PercentCompleteProperty, value);
        }

        public static string GetStatus(DependencyObject obj)
        {
            return (string)obj.GetValue(StatusProperty);
        }

        public static void SetStatus(DependencyObject obj, string value)
        {
            obj.SetValue(StatusProperty, value);
        }

        public static string GetGuid(DependencyObject obj)
        {
            return (string)obj.GetValue(GuidProperty);
        }

        public static void SetGuid(DependencyObject obj, string value)
        {
            obj.SetValue(GuidProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty; //
        //public static readonly DependencyProperty IsLoadedProperty;
        public static readonly DependencyProperty IsCanceledProperty; //
        public static readonly DependencyProperty PercentCompleteProperty; //
        public static readonly DependencyProperty StatusProperty;
        public static readonly DependencyProperty GuidProperty;


        

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
