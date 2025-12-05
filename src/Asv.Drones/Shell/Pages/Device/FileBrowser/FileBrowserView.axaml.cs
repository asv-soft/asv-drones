using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Asv.Drones;

public class FileBrowserViewConfig : IGridColumnLayoutConfig
{
    public IDictionary<string, ColumnConfig> Columns { get; set; } =
        new Dictionary<string, ColumnConfig>();
}

[ExportViewFor(typeof(FileBrowserViewModel))]
public partial class FileBrowserView : UserControl
{
    private readonly ILayoutService _layoutService;

    private int _realLocalFilesColumnOrder;
    private int _realGridSplitterColumnOrder;
    private int _realRemoteFilesColumnOrder;
    private FileBrowserViewConfig? _config;

    public FileBrowserView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public FileBrowserView(ILayoutService layoutService)
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

        _config = _layoutService.Get<FileBrowserViewConfig>(this);

        _realLocalFilesColumnOrder = Grid.GetColumn(LocalFilesColumn);
        _realGridSplitterColumnOrder = Grid.GetColumn(GridSplitterColumn);
        _realRemoteFilesColumnOrder = Grid.GetColumn(RemoteFilesColumn);

        if (_config.Columns.Count == 0)
        {
            return;
        }

        if (
            MainGrid.ColumnDefinitions.Count != _config.Columns.Keys.Count
            || _config.Columns.All(kvp => kvp.Value.Width.Value == 0)
            || !_config.Columns.TryGetValue(
                LocalFilesColumn.Name ?? string.Empty,
                out var localFilesColumn
            )
            || !_config.Columns.TryGetValue(
                GridSplitterColumn.Name ?? string.Empty,
                out var gridSplitterColumn
            )
            || !_config.Columns.TryGetValue(
                RemoteFilesColumn.Name ?? string.Empty,
                out var remoteFilesColumn
            )
            || _realLocalFilesColumnOrder != localFilesColumn.Order
            || _realGridSplitterColumnOrder != gridSplitterColumn.Order
            || _realRemoteFilesColumnOrder != remoteFilesColumn.Order
        )
        {
            _config = new FileBrowserViewConfig();
            return;
        }

        MainGrid.ColumnDefinitions[_realLocalFilesColumnOrder].Width = new GridLength(
            localFilesColumn.Width.Value,
            GridUnitType.Star
        );

        MainGrid.ColumnDefinitions[_realGridSplitterColumnOrder].Width = new GridLength(
            gridSplitterColumn.Width.Value,
            GridUnitType.Star
        );

        MainGrid.ColumnDefinitions[_realRemoteFilesColumnOrder].Width = new GridLength(
            remoteFilesColumn.Width.Value,
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
            LocalFilesColumn.Name is null
            || GridSplitterColumn.Name is null
            || RemoteFilesColumn.Name is null
        )
        {
            return;
        }

        _config.Columns = new Dictionary<string, ColumnConfig>
        {
            [LocalFilesColumn.Name] = new()
            {
                Order = _realLocalFilesColumnOrder,
                Width = new GridLengthConfig
                {
                    Value = MainGrid.ColumnDefinitions[_realLocalFilesColumnOrder].ActualWidth,
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
            [RemoteFilesColumn.Name] = new()
            {
                Order = _realRemoteFilesColumnOrder,
                Width = new GridLengthConfig
                {
                    Value = MainGrid.ColumnDefinitions[_realRemoteFilesColumnOrder].ActualWidth,
                    GridUnitType = GridUnitType.Star,
                },
            },
        };

        _layoutService.SetInMemory(this, _config);
    }

    private void TreeView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        if (sender is not TreeView tree)
        {
            return;
        }

        var hit = tree.InputHitTest(e.GetPosition(tree));

        if (IsInsideTreeViewItem(hit))
        {
            return;
        }

        tree.SelectedItem = null;
    }

    private static bool IsInsideTreeViewItem(IInputElement? hit)
    {
        var v = hit as Visual;
        while (v is not null && v is not TreeViewItem)
        {
            v = v.GetVisualParent();
        }

        return v is TreeViewItem;
    }
}

public static class TreeViewBehaviors
{
    public static readonly AttachedProperty<bool> IgnoreEnterProperty =
        AvaloniaProperty.RegisterAttached<TreeView, bool>("IgnoreEnter", typeof(TreeViewBehaviors));

    static TreeViewBehaviors()
    {
        IgnoreEnterProperty.Changed.AddClassHandler<TreeView>(OnIgnoreEnterChanged);
    }

    public static void SetIgnoreEnter(TreeView element, bool value) =>
        element.SetValue(IgnoreEnterProperty, value);

    public static bool GetIgnoreEnter(TreeView element) => element.GetValue(IgnoreEnterProperty);

    private static void OnIgnoreEnterChanged(TreeView tree, AvaloniaPropertyChangedEventArgs e)
    {
        var enabled = e.GetNewValue<bool>();
        if (enabled)
        {
            tree.AddHandler(InputElement.KeyDownEvent, OnTreeKeyDown, RoutingStrategies.Tunnel);
        }
        else
        {
            tree.RemoveHandler(InputElement.KeyDownEvent, OnTreeKeyDown);
        }
    }

    private static void OnTreeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
        }
    }
}
