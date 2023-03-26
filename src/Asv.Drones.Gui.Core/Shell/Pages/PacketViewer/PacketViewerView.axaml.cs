using System.ComponentModel.Composition;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(PacketViewerViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class PacketViewerView : ReactiveUserControl<PacketViewerViewModel>
{
    public PacketViewerView()
    {
        InitializeComponent();
    }

    private void SourcesChecked(object? sender, RoutedEventArgs e)
    {
        var filters = SourceFilters.Items.OfType<PacketFilterViewModel>();
        
        if (!filters.Any()) return;

        foreach (var filter in filters)
        {
            filter.IsChecked = true;
        }
    }

    private void SourcesUnchecked(object? sender, RoutedEventArgs e)
    {
        var filters = SourceFilters.Items.OfType<PacketFilterViewModel>();
        
        if (!filters.Any()) return;

        foreach (var filter in filters)
        {
            filter.IsChecked = false;
        }
    }

    private void TypesChecked(object? sender, RoutedEventArgs e)
    {
        var filters = TypeFilters.Items.OfType<PacketFilterViewModel>();

        if (!filters.Any()) return;

        foreach (var filter in filters)
        {
            filter.IsChecked = true;
        }
    }

    private void TypesUnchecked(object? sender, RoutedEventArgs e)
    {
        var filters = TypeFilters.Items.OfType<PacketFilterViewModel>();
        
        if (!filters.Any()) return;

        foreach (var filter in filters)
        {
            filter.IsChecked = false;
        }
    }
}