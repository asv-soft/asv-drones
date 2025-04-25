using System;
using System.Collections.Generic;
using Asv.Avalonia.Tree;
using ObservableCollections;

namespace Asv.Drones;

public class BrowserNode(
    IBrowserItemViewModel baseItem,
    IReadOnlyObservableList<IBrowserItemViewModel> source,
    Func<IBrowserItemViewModel, string> keySelector,
    Func<IBrowserItemViewModel, string> parentSelector,
    IComparer<IBrowserItemViewModel> comparer,
    CreateNodeDelegate<IBrowserItemViewModel, string> factory,
    ObservableTreeNode<IBrowserItemViewModel, string>? parentNode = null
)
    : ObservableTreeNode<IBrowserItemViewModel, string>(
        baseItem,
        source,
        keySelector,
        parentSelector,
        comparer,
        factory,
        parentNode
    );
