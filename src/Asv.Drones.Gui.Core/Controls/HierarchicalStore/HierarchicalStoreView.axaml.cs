using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

public partial class HierarchicalStoreView : ReactiveUserControl<HierarchicalStoreViewModel>
{
    public HierarchicalStoreView()
    {
        InitializeComponent();
    }
}