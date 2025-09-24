using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ReolMarket.Core
{
    internal class BoolVisibilityConverter
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
