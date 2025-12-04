using Asv.Avalonia;
using ObservableCollections;

namespace Asv.Drones;

public class BrowserTree(IReadOnlyObservableList<IBrowserItemViewModel> flatList, string rootKey)
    : ObservableTree<IBrowserItemViewModel, string>(
        flatList,
        rootKey,
        static x => x.Path,
        static x =>
        {
            var p = x.ParentPath ?? string.Empty;
            return p == x.Path ? string.Empty : p;
        },
        BrowserItemComparer.Instance,
        NodeFactory
    )
{
    private static readonly CreateNodeDelegate<IBrowserItemViewModel, string> NodeFactory = static (
        item,
        list,
        key,
        parent,
        comparer,
        factory,
        parentNode
    ) => new BrowserNode(item, list, key, parent, comparer, factory, parentNode);
}
