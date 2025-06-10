using System;
using System.Collections.Generic;
using Asv.Avalonia.Tree;
using ObservableCollections;

namespace Asv.Drones;

public class BrowserNode(
    IBrowserItem baseItem,
    IReadOnlyObservableList<IBrowserItem> source,
    Func<IBrowserItem, string> keySelector,
    Func<IBrowserItem, string> parentSelector,
    IComparer<IBrowserItem> comparer,
    CreateNodeDelegate<IBrowserItem, string> factory,
    ObservableTreeNode<IBrowserItem, string>? parentNode = null
)
    : ObservableTreeNode<IBrowserItem, string>(
        baseItem,
        source,
        keySelector,
        parentSelector,
        comparer,
        factory,
        parentNode
    );
