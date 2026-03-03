using System;
using System.Globalization;
using Xamarin.Forms;

namespace Lucky_wind.Converters
{
    /// <summary>
    /// Converts a bool to a Color using a ConverterParameter string.
    /// ConverterParameter format: "colorIfTrue|colorIfFalse"  (hex values, e.g. "#3211d4|#94a3b8")
    /// If the parameter is omitted, returns Black for false and Primary for true.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;

            if (parameter is string param && param.Contains("|"))
            {
                var parts = param.Split('|');
                string hex = boolValue ? parts[0] : parts[1];
                try { return Color.FromHex(hex); }
                catch { /* fall through to default */ }
            }

            return boolValue ? Color.FromHex("#3211d4") : Color.FromHex("#94a3b8");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
