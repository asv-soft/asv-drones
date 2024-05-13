using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

public partial class PacketViewerView : UserControl
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