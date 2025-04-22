using System.Collections.Generic;

namespace Asv.Drones;

public sealed class BrowserItemComparer : IComparer<IBrowserItem>
{
    public static readonly BrowserItemComparer Instance = new();

    private BrowserItemComparer() { }

    public int Compare(IBrowserItem? x, IBrowserItem? y)
    {
        if (x is DirectoryItem && y is not DirectoryItem)
        {
            return -1;
        }

        if (y is DirectoryItem && x is not DirectoryItem)
        {
            return 1;
        }

        return x switch
        {
            DirectoryItem dirX when y is DirectoryItem dirY =>
                DirectoryItemComparer.Instance.Compare(dirX, dirY),
            FileItem fileX when y is FileItem fileY => FileItemComparer.Instance.Compare(
                fileX,
                fileY
            ),
            _ => 0,
        };
    }
}

public sealed class DirectoryItemComparer : IComparer<DirectoryItem>
{
    public static readonly DirectoryItemComparer Instance = new();

    private DirectoryItemComparer() { }

    public int Compare(DirectoryItem? x, DirectoryItem? y)
    {
        switch (x)
        {
            case null when y == null:
                return 0;
            case null:
                return -1;
        }

        if (y == null)
        {
            return 1;
        }

        var idComparison = x.Id.CompareTo(y.Id);
        return idComparison != 0 ? idComparison : string.CompareOrdinal(x.ParentPath, y.ParentPath);
    }
}

public sealed class FileItemComparer : IComparer<FileItem>
{
    public static readonly FileItemComparer Instance = new();

    private FileItemComparer() { }

    public int Compare(FileItem? x, FileItem? y)
    {
        switch (x)
        {
            case null when y == null:
                return 0;
            case null:
                return -1;
        }

        if (y == null)
        {
            return 1;
        }

        var idComparison = x.Id.CompareTo(y.Id);
        if (idComparison != 0)
        {
            return idComparison;
        }

        var parentPathComparison = string.CompareOrdinal(x.ParentPath, y.ParentPath);
        if (parentPathComparison != 0)
        {
            return parentPathComparison;
        }

        if (x.Size.HasValue && y.Size.HasValue)
        {
            return x.Size.Value.CompareTo(y.Size.Value);
        }

        if (x.Size.HasValue)
        {
            return 1;
        }

        if (y.Size.HasValue)
        {
            return -1;
        }

        return 0;
    }
}
