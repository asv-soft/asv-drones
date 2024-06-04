using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Drones.Gui.Api
{
    public class AddPerсentDoubleConverter : IValueConverter
    {
        public static IValueConverter Instance { get; } = new AddPerсentDoubleConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string p && double.TryParse(p, out var add) && value is double v)
            {
                return v + v * add / 100;
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string p && double.TryParse(p, out var add) && value is double v)
            {
                return v - v * add / 100;
            }

            return value;
        }
    }
}