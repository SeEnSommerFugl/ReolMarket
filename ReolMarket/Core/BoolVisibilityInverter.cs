using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReolMarket.Core
{
    public class BoolVisibilityInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // Normal: true → Collapsed, false → Visible
                return b ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed; // default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                return v == Visibility.Collapsed;
            }

            return false;
        }
    }
}
