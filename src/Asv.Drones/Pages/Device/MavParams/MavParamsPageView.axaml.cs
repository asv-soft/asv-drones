using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using R3;

namespace Asv.Drones;

public struct GridLengthCfg
{
    public required double Width { get; init; }
    public required GridUnitType GridUnitType { get; init; }
}

public class MavParamsPageViewConfig
{
    public string? AppVersion { get; set; }
    public IList<GridLengthCfg> ColumnsWidth { get; set; } = new List<GridLengthCfg>();
}

[ExportViewFor(typeof(MavParamsPageViewModel))]
public partial class MavParamsPageView : UserControl
{
    private readonly ILayoutService _layoutService;
    private readonly string? _appVersion;

    private MavParamsPageViewConfig? _config;

    public MavParamsPageView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        _appVersion = NullAppInfo.Instance.Version;
    }

    [ImportingConstructor]
    public MavParamsPageView(ILayoutService layoutService)
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
        _config = _layoutService.Get<MavParamsPageViewConfig>(this);

        if (_config.AppVersion is not null && _config.AppVersion != _appVersion)
        {
            _config = new MavParamsPageViewConfig();
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
