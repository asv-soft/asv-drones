using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using R3;

namespace Asv.Drones;

public class MavParamsPageViewConfig : IGridColumnLayoutConfig
{
    public IDictionary<string, ColumnConfig> Columns { get; set; } =
        new Dictionary<string, ColumnConfig>();
}

[ExportViewFor(typeof(MavParamsPageViewModel))]
public partial class MavParamsPageView : UserControl
{
    private readonly ILayoutService _layoutService;

    private int _realAllParamsColumnOrder;
    private int _realGridSplitterColumnOrder;
    private int _realViewedParamsColumnOrder;
    private MavParamsPageViewConfig? _config;

    public MavParamsPageView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public MavParamsPageView(ILayoutService layoutService)
    {
        _layoutService = layoutService;
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LoadLayout();
        base.OnAttachedToVisualTree(e);
    }

    private void GridSplitter_Dragged(object? sender, VectorEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        if (sender is not GridSplitter)
        {
            return;
        }

        SaveLayout();
    }

    private void LoadLayout()
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        _config = _layoutService.Get<MavParamsPageViewConfig>(this);

        _realAllParamsColumnOrder = Grid.GetColumn(AllParamsColumn);
        _realGridSplitterColumnOrder = Grid.GetColumn(GridSplitterColumn);
        _realViewedParamsColumnOrder = Grid.GetColumn(ViewedParamsColumn);

        if (_config.Columns.Count == 0)
        {
            return;
        }

        if (
            MainGrid.ColumnDefinitions.Count != _config.Columns.Keys.Count
            || _config.Columns.All(kvp => kvp.Value.Width.Value == 0)
            || !_config.Columns.TryGetValue(
                AllParamsColumn.Name ?? string.Empty,
                out var allParamsColumn
            )
            || !_config.Columns.TryGetValue(
                GridSplitterColumn.Name ?? string.Empty,
                out var gridSplitterColumn
            )
            || !_config.Columns.TryGetValue(
                ViewedParamsColumn.Name ?? string.Empty,
                out var viewedParamsColumn
            )
            || _realAllParamsColumnOrder != allParamsColumn.Order
            || _realGridSplitterColumnOrder != gridSplitterColumn.Order
            || _realViewedParamsColumnOrder != viewedParamsColumn.Order
        )
        {
            _config = new MavParamsPageViewConfig();
            return;
        }

        MainGrid.ColumnDefinitions[_realAllParamsColumnOrder].Width = new GridLength(
            allParamsColumn.Width.Value,
            GridUnitType.Star
        );

        MainGrid.ColumnDefinitions[_realGridSplitterColumnOrder].Width = new GridLength(
            gridSplitterColumn.Width.Value,
            GridUnitType.Star
        );

        MainGrid.ColumnDefinitions[_realViewedParamsColumnOrder].Width = new GridLength(
            viewedParamsColumn.Width.Value,
            GridUnitType.Star
        );
    }

    private void SaveLayout()
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        if (_config is null)
        {
            return;
        }

        if (DataContext is null)
        {
            return;
        }

        if (
            AllParamsColumn.Name is null
            || GridSplitterColumn.Name is null
            || ViewedParamsColumn.Name is null
        )
        {
            return;
        }

        _config.Columns = new Dictionary<string, ColumnConfig>
        {
            [AllParamsColumn.Name] = new()
            {
                Order = _realAllParamsColumnOrder,
                Width = new GridLengthConfig
                {
                    Value = MainGrid.ColumnDefinitions[_realAllParamsColumnOrder].ActualWidth,
                    GridUnitType = GridUnitType.Star,
                },
            },
            [GridSplitterColumn.Name] = new()
            {
                Order = _realGridSplitterColumnOrder,
                Width = new GridLengthConfig
                {
                    Value = MainGrid.ColumnDefinitions[_realGridSplitterColumnOrder].ActualWidth,
                    GridUnitType = GridUnitType.Star,
                },
            },
            [ViewedParamsColumn.Name] = new()
            {
                Order = _realViewedParamsColumnOrder,
                Width = new GridLengthConfig
                {
                    Value = MainGrid.ColumnDefinitions[_realViewedParamsColumnOrder].ActualWidth,
                    GridUnitType = GridUnitType.Star,
                },
            },
        };

        _layoutService.SetInMemory(this, _config);
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
