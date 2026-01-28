using System;
using System.Composition;
using System.Globalization;
using Asv.Avalonia;
using Avalonia.Data.Converters;

namespace Asv.Drones;

/// <summary>
/// Simple converter from Crc32Status to AsvColorKind
/// </summary>
public class Crc32StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Crc32Status status)
        {
            return status switch
            {
                Crc32Status.Correct => AsvColorKind.Success,
                Crc32Status.Incorrect => AsvColorKind.Error,
                Crc32Status.Default => AsvColorKind.Unknown,
                _ => AsvColorKind.None,
            };
        }

        return AsvColorKind.None;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is AsvColorKind color)
        {
            return color switch
            {
                AsvColorKind.Success => Crc32Status.Correct,
                AsvColorKind.Error => Crc32Status.Incorrect,
                _ => Crc32Status.Default,
            };
        }

        return Crc32Status.Default;
    }
}
