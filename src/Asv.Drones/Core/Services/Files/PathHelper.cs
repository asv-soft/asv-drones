using System;
using Asv.Avalonia;

namespace Asv.Drones;

public static class PathHelper
{
    public static NavigationId EncodePathToId(string path)
    {
        var utf8 = System.Text.Encoding.UTF8.GetBytes(path);
        return Convert
            .ToBase64String(utf8)
            .Replace('+', '.')
            .Replace('/', '_')
            .Replace("=", string.Empty);
    }
}
