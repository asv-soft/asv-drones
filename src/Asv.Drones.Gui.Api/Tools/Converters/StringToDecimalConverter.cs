using Avalonia.Data.Converters;
using System.Globalization;

namespace Asv.Drones.Gui.Api
{
    public class StringToDecimalConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
                return decimalValue; 
            return 0m; 
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue.ToString("0.00", CultureInfo.InvariantCulture);
            return string.Empty;
        }
    }
}