using System;
using Asv.Modeling;

namespace Asv.Drones;

public static class PathHelper
{
    public static NavId EncodePathToId(string path)
    {
        var utf8 = System.Text.Encoding.UTF8.GetBytes(path);
        var id = Convert
            .ToBase64String(utf8)
            .Replace('+', '.')
            .Replace('/', '_')
            .Replace("=", string.Empty);
        return new NavId(id);
    }
}
