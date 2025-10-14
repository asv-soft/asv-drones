using System.Composition;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Drones;

public sealed class PacketViewerViewConfig
{
    public bool IsSourcesExpanded { get; set; } = true;
    public bool IsTypesExpanded { get; set; } = true;
}

[ExportViewFor(typeof(PacketViewerViewModel))]
public partial class PacketViewerView : UserControl
{
    private readonly ILayoutService _layoutService;

    private PacketViewerViewConfig? _config;

    public PacketViewerView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public PacketViewerView(ILayoutService layoutService)
    {
        _layoutService = layoutService;
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LoadLayout();
        base.OnAttachedToVisualTree(e);
    }

    private void Expander_StateChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not Expander exp)
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

        _config = _layoutService.Get<PacketViewerViewConfig>(this);
        SourcesExpander.IsExpanded = _config.IsSourcesExpanded;
        TypesExpander.IsExpanded = _config.IsTypesExpanded;
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

        if (!HasChanges())
        {
            return;
        }

        _config.IsSourcesExpanded = SourcesExpander.IsExpanded;
        _config.IsTypesExpanded = TypesExpander.IsExpanded;
        _layoutService.SetInMemory(this, _config);
    }

    private bool HasChanges()
    {
        if (
            _config?.IsSourcesExpanded == SourcesExpander.IsExpanded
            && _config.IsTypesExpanded == TypesExpander.IsExpanded
        )
        {
            return false;
        }

        return true;
    }
}
