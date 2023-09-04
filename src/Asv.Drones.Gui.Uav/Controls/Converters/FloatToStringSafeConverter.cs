using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Asv.Drones.Gui.Uav;

public class FloatToStringSafeConverter : IValueConverter
{
    public static IValueConverter Instance { get; } = new FloatToStringSafeConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null && value is float val)
        {
            return val.ToString();
        }
        return "0";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        value = value.ToString().Replace(".", ",");
        if (float.TryParse(value.ToString(), out var result))
        {
            return result;
        }
        return 0f;
    }
}

