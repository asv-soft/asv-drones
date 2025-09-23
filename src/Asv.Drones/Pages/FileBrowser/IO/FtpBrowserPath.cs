namespace Asv.Drones;

/// <summary>
/// Utility methods for working with FTP browser paths.
/// </summary>
public static class FtpBrowserPath
{
    /// <summary>
    /// Normalizes a path ensuring it ends with the correct separator if it is a directory,
    /// or trims trailing separators if it is a file.
    /// </summary>
    /// <param name="path">The original path string.</param>
    /// <param name="isDirectory">True if the path is a directory; false if it is a file.</param>
    /// <param name="sep">Directory separator character.</param>
    /// <returns>A normalized path string.</returns>
    public static string Normalize(string path, bool isDirectory, char sep) =>
        isDirectory ? EnsureDir(path, sep) : EnsureFile(path, sep);

    /// <summary>
    /// Combines a parent directory with a child directory name and ensures the result ends with a separator.
    /// </summary>
    /// <param name="parentDir">The parent directory path.</param>
    /// <param name="name">The child directory name.</param>
    /// <param name="sep">Directory separator character.</param>
    /// <returns>A full directory path ending with a separator.</returns>
    public static string CombineDir(string parentDir, string name, char sep) =>
        EnsureDir(parentDir, sep) + name + sep;

    /// <summary>
    /// Combines a parent directory with a file name.
    /// </summary>
    /// <param name="parentDir">The parent directory path.</param>
    /// <param name="name">The file name.</param>
    /// <param name="sep">Directory separator character.</param>
    /// <returns>A full file path.</returns>
    public static string CombineFile(string parentDir, string name, char sep) =>
        EnsureDir(parentDir, sep) + name;

    /// <summary>
    /// Gets the parent directory of the given path.
    /// </summary>
    /// <param name="path">The full path string.</param>
    /// <param name="sep">Directory separator character.</param>
    /// <returns>
    /// Parent directory path (ending with a separator), or empty string if the path is root or invalid.
    /// </returns>
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

    /// <summary>
    /// Extracts the name (file or directory) from the given path.
    /// </summary>
    /// <param name="path">The full path string.</param>
    /// <param name="sep">Directory separator character.</param>
    /// <returns>The file or directory name, or empty string if invalid.</returns>
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

    /// <summary>
    /// Ensures that the given path represents a directory (ends with a separator).
    /// </summary>
    private static string EnsureDir(string path, char sep)
    {
        if (string.IsNullOrWhiteSpace(path) || (path.Length == 1 && path[0] == sep))
        {
            return sep.ToString();
        }

        return path[^1] == sep ? path : path + sep;
    }

    /// <summary>
    /// Ensures that the given path represents a file (removes trailing separators).
    /// </summary>
    private static string EnsureFile(string path, char sep)
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
}
