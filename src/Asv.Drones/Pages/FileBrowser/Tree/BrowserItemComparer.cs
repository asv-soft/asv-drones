using System.Collections.Generic;

namespace Asv.Drones;

public sealed class BrowserItemComparer : IComparer<IBrowserItemViewModel>
{
    public static readonly BrowserItemComparer Instance = new();

    private BrowserItemComparer() { }

    public int Compare(IBrowserItemViewModel? x, IBrowserItemViewModel? y)
    {
        if (x is DirectoryItemViewModel && y is not DirectoryItemViewModel)
        {
            return -1;
        }

        if (y is DirectoryItemViewModel && x is not DirectoryItemViewModel)
        {
            return 1;
        }

        return x switch
        {
            DirectoryItemViewModel dirX when y is DirectoryItemViewModel dirY =>
                DirectoryItemComparer.Instance.Compare(dirX, dirY),
            FileItemViewModel fileX when y is FileItemViewModel fileY =>
                FileItemComparer.Instance.Compare(fileX, fileY),
            _ => 0,
        };
    }
}

public sealed class DirectoryItemComparer : IComparer<DirectoryItemViewModel>
{
    public static readonly DirectoryItemComparer Instance = new();

    private DirectoryItemComparer() { }

    public int Compare(DirectoryItemViewModel? x, DirectoryItemViewModel? y)
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

public sealed class FileItemComparer : IComparer<FileItemViewModel>
{
    public static readonly FileItemComparer Instance = new();

    private FileItemComparer() { }

    public int Compare(FileItemViewModel? x, FileItemViewModel? y)
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
