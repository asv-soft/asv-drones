using Asv.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using R3;

namespace Asv.Drones;

[ExportViewFor(typeof(MavParamsPageViewModel))]
public partial class MavParamsPageView : UserControl
{
    public MavParamsPageView()
    {
        InitializeComponent();
    }

    private void ItemDockPanel_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        if (DataContext is not MavParamsPageViewModel viewModel)
        {
            return;
        }

        if (sender is not DockPanel { DataContext: { } item })
        {
            return;
        }

        if (ReferenceEquals(viewModel.SelectedItem.Value, item))
        {
            viewModel.SelectedItem.Value?.PinItem.Execute(Unit.Default);
        }
    }

    private void Button_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }
}
