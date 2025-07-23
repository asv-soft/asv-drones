using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Drones;

public class BrowserTree(IReadOnlyObservableList<IBrowserItemViewModel> flatList, string rootKey)
    : ObservableTree<IBrowserItemViewModel, string>(
        flatList,
        rootKey,
        x => x.Path,
        x => x.ParentPath ?? string.Empty,
        BrowserItemComparer.Instance,
        (item, list, key, parent, comparer, transform, node) =>
            new BrowserNode(item, list, key, parent, comparer, transform, node) // TODO: Fix potential memory loss
    ) { }
