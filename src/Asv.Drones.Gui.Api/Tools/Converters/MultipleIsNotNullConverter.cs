using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Drones.Gui.Api;

public class MultipleIsNotNullConverter : IMultiValueConverter
{
    public static IMultiValueConverter Instance { get; } = new MultipleIsNotNullConverter();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.All(value => value != null);
    }

    public object ConvertBack(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}