using System;
using System.Globalization;
using Xamarin.Forms;

namespace Lucky_wind.Converters
{
    /// <summary>
    /// Convierte un valor bool a su inverso. Útil para bindear IsPassword a IsPasswordVisible.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }
    }
}
