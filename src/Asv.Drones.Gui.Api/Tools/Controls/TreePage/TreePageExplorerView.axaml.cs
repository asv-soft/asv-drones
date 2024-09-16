using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.ReactiveUI;
using DynamicData.Binding;

namespace Asv.Drones.Gui.Api;

public partial class TreePageExplorerView : ReactiveUserControl<TreePageExplorerViewModel>
{
    private IDisposable? _subscribe;
    private double? _lastWidth = null;

    public TreePageExplorerView()
    {
        InitializeComponent();
        this.WhenValueChanged(x => x.ViewModel).Subscribe(OnViewModel);
    }

    private void OnViewModel(TreePageExplorerViewModel? vm)
    {
        if (vm == null) return;
        _subscribe?.Dispose();
        _subscribe = vm.WhenValueChanged(x => x.IsCompactMode, false)
            .Subscribe(OnIsCompactModeChanged);
    }

    private void OnIsCompactModeChanged(bool compactMode)
    {
        var column = PART_Grid.ColumnDefinitions.FirstOrDefault();
        if (compactMode && column != null)
        {
            column.Width = new GridLength(0, GridUnitType.Auto);
        }
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (_lastWidth == null)
        {
            if (PART_TitleGrid.ColumnDefinitions[2].ActualWidth < 8)
            {
                if (ViewModel != null) ViewModel.IsTitleCompactMode = true;
                // replace large control
                _lastWidth = PART_TitleGrid.Bounds.Width;
            }
        }
        else
        {
            if (PART_TitleGrid.Bounds.Width > _lastWidth)
            {
                if (ViewModel != null) ViewModel.IsTitleCompactMode = false;
                _lastWidth = null;
            }
        }
    }
}