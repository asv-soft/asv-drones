using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Asv.Drones.Gui.Api
{
    public class MaterialIconConverter : IValueConverter
    {
        public static IValueConverter Instance { get; } = new MaterialIconConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MaterialIconKind kind)
            {
                return new MaterialIcon
                {
                    Kind = kind
                };
            }

            if (value is string str)
            {
                if (Enum.TryParse(str, true, out kind))
                {
                    return new MaterialIcon
                    {
                        Kind = kind
                    };
                }
            }

            return new MaterialIcon();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}