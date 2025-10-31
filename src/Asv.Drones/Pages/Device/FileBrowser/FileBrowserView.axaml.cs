using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Asv.Drones;

public class FileBrowserViewConfig : IAppVersionLayoutConfig, IColumnWidthLayoutConfig
{
    public string? AppVersion { get; set; }
    public IList<GridLengthCfg> ColumnsWidth { get; set; } = new List<GridLengthCfg>();
}

[ExportViewFor(typeof(FileBrowserViewModel))]
public partial class FileBrowserView : UserControl
{
    private readonly ILayoutService _layoutService;
    private readonly string? _appVersion;

    private FileBrowserViewConfig? _config;

    public FileBrowserView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        _appVersion = NullAppInfo.Instance.Version;
    }

    [ImportingConstructor]
    public FileBrowserView(ILayoutService layoutService)
    {
        _layoutService = layoutService;
        if (!Design.IsDesignMode)
        {
            _appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? null;
        }

        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LoadLayout();
        base.OnAttachedToVisualTree(e);
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
        _config = _layoutService.Get<FileBrowserViewConfig>(this);

        if (_config.AppVersion is not null && _config.AppVersion != _appVersion)
        {
            _config = new FileBrowserViewConfig();
            return;
        }

        for (var i = 0; i < _config.ColumnsWidth.Count; i++)
        {
            MainGrid.ColumnDefinitions[i].Width = new GridLength(
                _config.ColumnsWidth[i].Width,
                _config.ColumnsWidth[i].GridUnitType
            );
        }
    }

    private void SaveLayout()
    {
        if (_config is null)
        {
            return;
        }

        if (DataContext is null)
        {
            return;
        }

        _config.AppVersion = _appVersion;
        _config.ColumnsWidth = MainGrid
            .ColumnDefinitions.Select(c => new GridLengthCfg
            {
                Width = c.Width.Value,
                GridUnitType = c.Width.GridUnitType,
            })
            .ToList();
        _layoutService.SetInMemory(this, _config);
    }

    private void TreeView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
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
