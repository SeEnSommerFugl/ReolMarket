using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReolMarket.Core
{
    public class HiddenToShown : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If bound value is null → collapsed (not visible)
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
