using System;
using System.Collections.Generic;
using System.Linq;

namespace Asv.Drones;

/// <inheritdoc />
public sealed class RenameRelinker : IRenameRelinker
{
    public void RelinkDirectory(
        IEnumerable<IBrowserItemViewModel> items,
        string oldDirectoryPath,
        string newDirectoryPath,
        char sep
    )
    {
        ArgumentNullException.ThrowIfNull(items);

        if (string.IsNullOrEmpty(oldDirectoryPath))
        {
            throw new ArgumentException(@"Old path is empty", nameof(oldDirectoryPath));
        }

        if (string.IsNullOrEmpty(newDirectoryPath))
        {
            throw new ArgumentException(@"New path is empty", nameof(newDirectoryPath));
        }

        var oldC = PathEx.Canonical(oldDirectoryPath, sep);
        var newC = PathEx.Canonical(newDirectoryPath, sep);

        var itemsArray = items as IBrowserItemViewModel[] ?? items.ToArray();

        // Update the directory item itself
        foreach (
            var item in itemsArray.Where(x => string.Equals(x.Path, oldC, StringComparison.Ordinal))
        )
        {
            item.Path = newC;
            item.ParentPath = PathEx.ParentOf(newC, sep);
            item.Name = PathEx.LastSegment(newC, sep);
        }

        // Update descendants
        foreach (var item in itemsArray.Where(x => PathEx.IsDescendantOf(oldC, x.Path, sep)))
        {
            var newPath = PathEx.ReplacePrefixNormalized(item.Path, oldC, newC, sep);
            item.Path = newPath;

            if (!string.IsNullOrEmpty(item.ParentPath))
            {
                item.ParentPath = PathEx.ReplacePrefixNormalized(item.ParentPath, oldC, newC, sep);
            }
        }
    }

    public string RelinkItem(IBrowserItemViewModel item, string newName, char sep)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException(@"New name is empty", nameof(newName));
        }

        var parent = item.ParentPath ?? string.Empty;
        var parentWithSep = PathEx.WithSep(parent, sep);
        var newPath = parentWithSep + newName;

        item.Path = PathEx.Canonical(newPath, sep);
        item.Name = newName;

        return item.Path;
    }
}
