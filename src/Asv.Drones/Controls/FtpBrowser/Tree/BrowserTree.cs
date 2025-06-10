using Asv.Avalonia.Tree;
using ObservableCollections;

namespace Asv.Drones;

public class BrowserTree(IReadOnlyObservableList<IBrowserItem> flatList, string rootKey)
    : ObservableTree<IBrowserItem, string>(
        flatList,
        rootKey,
        x => x.Path,
        x => x.ParentPath ?? string.Empty,
        BrowserItemComparer.Instance,
        (item, list, key, parent, comparer, transform, node) =>
            new BrowserNode(item, list, key, parent, comparer, transform, node)
    );
