using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReolMarket.Core
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // Normal: true → Visible, false → Collapsed
                return b ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed; // default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                return v == Visibility.Visible;
            }

            return false;
        }
    }
}
