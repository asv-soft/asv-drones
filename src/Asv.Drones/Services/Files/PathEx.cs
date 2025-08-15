using System;

namespace Asv.Drones;

internal static class PathEx
{
    public static string Canonical(string path, char sep)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (path.Length == 1 && path[0] == sep)
        {
            return path;
        }

        var i = path.Length;
        while (i > 1 && path[i - 1] == sep)
        {
            i--;
        }

        return path.AsSpan(0, i).ToString();
    }

    public static string WithSep(string path, char sep)
    {
        var c = Canonical(path, sep);
        return c.Length == 1 && c[0] == sep ? c
            : c.EndsWith(sep) ? c
            : c + sep;
    }

    public static bool IsDescendantOf(string ancestor, string path, char sep)
    {
        var prefix = WithSep(ancestor, sep);
        return path.StartsWith(prefix, StringComparison.Ordinal);
    }

    public static string ReplacePrefixNormalized(
        string value,
        string oldPrefix,
        string newPrefix,
        char sep
    )
    {
        var oldWith = WithSep(oldPrefix, sep);
        if (!value.StartsWith(oldWith, StringComparison.Ordinal))
        {
            return value;
        }

        var rest = value.AsSpan(oldWith.Length);
        return string.Concat(WithSep(newPrefix, sep).AsSpan(), rest);
    }

    public static string LastSegment(string path, char sep)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var c = Canonical(path, sep);
        var idx = c.LastIndexOf(sep);
        return idx < 0 ? c : c[(idx + 1)..];
    }

    public static string? ParentOf(string path, char sep)
    {
        var c = Canonical(path, sep);
        if (c.Length == 1 && c[0] == sep)
        {
            return string.Empty;
        }

        var idx = c.LastIndexOf(sep);
        return idx <= 0 ? string.Empty : c[..idx];
    }

    public static string NameOf(string? path, char sep)
    {
        if (string.IsNullOrEmpty(path) || (path.Length == 1 && path[0] == sep))
        {
            return string.Empty;
        }

        var end = path.Length;
        while (end > 1 && path[end - 1] == sep)
        {
            end--;
        }

        if (end <= 0)
        {
            return string.Empty;
        }

        var idx = path.LastIndexOf(sep, end - 1);
        return idx < 0 ? path[..end] : path.Substring(idx + 1, end - (idx + 1));
    }

    public static string NameOf(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var end = path.Length;
        while (end > 0 && (path[end - 1] == '/' || path[end - 1] == '\\'))
        {
            if (end == 3 && char.IsLetter(path[0]) && path[1] == ':')
            {
                break;
            }
            if (end == 1)
            {
                break;
            }

            end--;
        }
        if (end <= 0)
        {
            return string.Empty;
        }

        var i1 = path.LastIndexOf('/', end - 1);
        var i2 = path.LastIndexOf('\\', end - 1);
        var idx = Math.Max(i1, i2);
        return idx < 0 ? path[..end] : path.Substring(idx + 1, end - (idx + 1));
    }
}
