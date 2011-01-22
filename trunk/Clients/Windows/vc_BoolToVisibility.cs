using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace WindowsClient
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class vc_BoolToVisibility : IValueConverter
    {
        public static vc_BoolToVisibility Instance = new vc_BoolToVisibility();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = ((bool)value);

            if (parameter is bool)
            {
                if ((bool)parameter)
                {
                    bValue = !bValue;
                }
            }

            if (bValue)
            {
                return (Visibility.Visible);
            }
            else
            {
                return (Visibility.Collapsed);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vValue = ((Visibility)value);
            if (vValue == Visibility.Hidden)
            {
                vValue = Visibility.Collapsed;
            }

            if (parameter is bool)
            {
                if ((bool)parameter)
                {
                    if (vValue == Visibility.Visible)
                    {
                        vValue = Visibility.Collapsed;
                    }
                    else
                    {
                        vValue = Visibility.Visible;
                    }
                }
            }
            return (vValue == Visibility.Visible);
        }
    }
}
