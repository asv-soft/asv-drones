using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Drones.Gui.Api
{
    public class AddDoubleConverter : IValueConverter
    {
        public static IValueConverter Instance { get; } = new AddDoubleConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string p && double.TryParse(p, out var add) && value is double v)
            {
                return add + v;
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string p && double.TryParse(p, out var add) && value is double v)
            {
                return add - v;
            }

            return value;
        }
    }
}