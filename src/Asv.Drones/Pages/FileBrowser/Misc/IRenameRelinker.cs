using System.Collections.Generic;

namespace Asv.Drones;

/// <summary>
/// <para>
/// Pure relinking service that updates in-memory items after a rename operation:
/// <para>
/// - File rename: update Path/ParentPath/Header of the renamed item
/// </para>
/// - Directory rename: update Path/ParentPath/Header for the directory and all its descendants
/// </para>
/// <para>This class does not perform any IO. It only adjusts view models to keep the tree consistent.</para>
/// </summary>
public interface IRenameRelinker
{
    /// <summary>
    /// Relink directory <paramref name="oldDirectoryPath"/> to <paramref name="newDirectoryPath"/>:
    /// updates the directory item itself and all descendants in <paramref name="items"/>.
    /// </summary>
    /// <param name="items">Collection of the file system entries.</param>
    /// <param name="oldDirectoryPath">Old path to a directory.</param>
    /// <param name="newDirectoryPath">New path to a directory.</param>
    /// <param name="sep">File system separator.</param>
    void RelinkDirectory(
        IEnumerable<IBrowserItemViewModel> items,
        string oldDirectoryPath,
        string newDirectoryPath,
        char sep
    );

    /// <summary>
    /// Relink a single file/directory item to a new name inside the same parent.
    /// </summary>
    /// <param name="item">Item to be relinked.</param>
    /// <param name="newName">Item's new name.</param>
    /// <param name="sep">File system separator.</param>
    /// <returns>The new absolute path computed for the item.</returns>
    string RelinkItem(IBrowserItemViewModel item, string newName, char sep);
}
