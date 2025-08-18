using System;

namespace Asv.Drones;

public static class BrowserPathRules
{
    public static string EnsureDir(string path, char sep)
    {
        if (string.IsNullOrWhiteSpace(path) || (path.Length == 1 && path[0] == sep))
        {
            return sep.ToString();
        }

        return path[^1] == sep ? path : path + sep;
    }

    public static string EnsureFile(string path, char sep)
    {
        if (string.IsNullOrWhiteSpace(path) || (path.Length == 1 && path[0] == sep))
        {
            return string.Empty;
        }

        var end = path.Length;
        while (end > 1 && path[end - 1] == sep)
        {
            end--;
        }

        return path[..end];
    }

    public static string Normalize(string path, bool isDirectory, char sep) =>
        isDirectory ? EnsureDir(path, sep) : EnsureFile(path, sep);

    public static string CombineDir(string parentDir, string name, char sep) =>
        EnsureDir(parentDir, sep) + name + sep;

    public static string CombineFile(string parentDir, string name, char sep) =>
        EnsureDir(parentDir, sep) + name;

    public static string ParentDirOf(string path, char sep)
    {
        if (string.IsNullOrWhiteSpace(path) || (path.Length == 1 && path[0] == sep))
        {
            return string.Empty;
        }

        var isDir = path[^1] == sep;
        var p = isDir ? path.TrimEnd(sep) : path;

        var idx = p.LastIndexOf(sep);
        return idx switch
        {
            < 0 => string.Empty,
            0 => sep.ToString(),
            _ => p[..(idx + 1)],
        };
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

        var idx = path.LastIndexOf(sep, end - 1);
        return idx < 0 ? path[..end] : path.Substring(idx + 1, end - (idx + 1));
    }

    public static bool IsDescendantOf(string ancestorDir, string path, char sep)
    {
        var prefix = EnsureDir(ancestorDir, sep);
        return Normalize(path, path.EndsWith(sep), sep)
            .StartsWith(prefix, StringComparison.Ordinal);
    }

    public static string ReplacePrefix(string value, string oldDir, string newDir, char sep)
    {
        var oldPref = EnsureDir(oldDir, sep);
        if (!value.StartsWith(oldPref, StringComparison.Ordinal))
        {
            return value;
        }

        var rest = value.AsSpan(oldPref.Length);
        return EnsureDir(newDir, sep) + rest.ToString();
    }
}
