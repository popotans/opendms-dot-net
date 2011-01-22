using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace WindowsClient
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class vc_FlipBool : IValueConverter
    {
        public static vc_FlipBool Instance = new vc_FlipBool();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }
    }
}
