using Asv.Avalonia;
using Avalonia;
using Avalonia.Media;

namespace Asv.Drones.Api;

public static class AsvColorKindMixin
{
    public static IBrush ToBrush(this AsvColorKind color)
    {
        var resourceKey = GetForegroundBrushResourceKey(color);
        if (
            resourceKey is not null
            && Application.Current?.TryGetResource(resourceKey, null, out var resource) == true
            && resource is IBrush brush
        )
        {
            return brush;
        }

        return Brushes.Aquamarine;
    }

    private static string? GetForegroundBrushResourceKey(AsvColorKind color)
    {
        var colorName = Enum.GetName(color);
        return colorName is null or nameof(AsvColorKind.None)
            ? null
            : $"AsvForeground{colorName}Brush";
    }
}
