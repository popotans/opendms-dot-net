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
