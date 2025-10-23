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

    [ImportingConstructor]
    public PacketViewerView(ILayoutService layoutService)
        : this()
    {
        _layoutService = layoutService;
    }

    public PacketViewerView()
    {
        if (Design.IsDesignMode)
        {
            _layoutService = NullLayoutService.Instance;
        }

        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LoadLayout();
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SaveLayout();
        base.OnDetachedFromVisualTree(e);
    }

    private void LoadLayout()
    {
        _config = _layoutService.Get<PacketViewerViewConfig>(this);
        SourcesExpander.IsExpanded = _config.IsSourcesExpanded;
        TypesExpander.IsExpanded = _config.IsTypesExpanded;
    }

    private void SaveLayout()
    {
        if (_config is null)
        {
            return;
        }

        _config.IsSourcesExpanded = SourcesExpander.IsExpanded;
        _config.IsTypesExpanded = TypesExpander.IsExpanded;
        _layoutService.SetInMemory(this, _config);
    }

    private void Expander_StateChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not Expander exp)
        {
            return;
        }

        SaveLayout();
    }
}
