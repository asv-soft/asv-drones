using System;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using R3;

namespace Asv.Drones;

public partial class MavParamsPageView : UserControl
{
    private const int AllParamsColumnIndex = 0;

    private readonly IDisposable? _columnWidthLayout;

    public MavParamsPageView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            return;
        }

        _columnWidthLayout = this.RegisterGridColumnPixelWidth(
            $"{MavParamsPageViewModel.PageId}.columnWidth",
            MainGrid,
            AllParamsColumnIndex
        );
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _columnWidthLayout?.Dispose();
        base.OnDetachedFromVisualTree(e);
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
