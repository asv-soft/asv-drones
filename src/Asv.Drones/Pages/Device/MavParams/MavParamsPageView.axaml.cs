using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using R3;

namespace Asv.Drones;

public struct GridLengthCfg
{
    public double Width { get; init; }
    public GridUnitType GridUnitType { get; init; }
}

public class MavParamsPageViewConfig
{
    public IList<GridLengthCfg> ColumnsWidth { get; set; } = new List<GridLengthCfg>();
}

[ExportViewFor(typeof(MavParamsPageViewModel))]
public partial class MavParamsPageView : UserControl
{
    private readonly ILayoutService _layoutService;

    private Grid? _mainGrid;
    private MavParamsPageViewConfig? _config;

    [ImportingConstructor]
    public MavParamsPageView(ILayoutService layoutService)
        : this()
    {
        _layoutService = layoutService;
    }

    public MavParamsPageView()
    {
        if (Design.IsDesignMode)
        {
            _layoutService = NullLayoutService.Instance;
        }

        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _mainGrid = FindParentGridOfSplitter();
        LoadLayout();
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SaveLayout();
        base.OnDetachedFromVisualTree(e);
    }

    private void GridSplitter_Dragged(object? sender, VectorEventArgs e)
    {
        if (sender is not GridSplitter)
        {
            return;
        }

        SaveLayout();
    }

    private void LoadLayout()
    {
        _config = _layoutService.Get<MavParamsPageViewConfig>(this);

        if (_mainGrid is null)
        {
            return;
        }

        for (var i = 0; i < _config.ColumnsWidth.Count; i++)
        {
            _mainGrid.ColumnDefinitions[i].Width = new GridLength(
                _config.ColumnsWidth[i].Width,
                _config.ColumnsWidth[i].GridUnitType
            );
        }
    }

    private void SaveLayout()
    {
        if (_mainGrid is null)
        {
            return;
        }

        if (_config is null)
        {
            return;
        }

        _config.ColumnsWidth = _mainGrid
            .ColumnDefinitions.Select(c => new GridLengthCfg
            {
                Width = c.Width.Value,
                GridUnitType = c.Width.GridUnitType,
            })
            .ToList();
        _layoutService.SetInMemory(this, _config);
    }

    private Grid? FindParentGridOfSplitter()
    {
        return MainGridSplitter?.GetVisualParent() as Grid
            ?? MainGridSplitter?.GetVisualAncestors().OfType<Grid>().FirstOrDefault();
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
