using System.ComponentModel.Composition;
using Avalonia.Controls;
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

    private void ToggleButton_OnChecked(object? sender, RoutedEventArgs e)
    {
        var filters = Filters.Items.OfType<PacketFilterViewModel>();

        foreach (var filter in filters)
        {
            filter.IsChecked = true;
        }
    }

    private void ToggleButton_OnUnchecked(object? sender, RoutedEventArgs e)
    {
        var filters = Filters.Items.OfType<PacketFilterViewModel>();

        foreach (var filter in filters)
        {
            filter.IsChecked = false;
        }
    }
}