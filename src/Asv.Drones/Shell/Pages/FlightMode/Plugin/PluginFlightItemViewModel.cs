using System.Collections.Generic;
using Asv.Avalonia;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Drones;

public class PluginFlightItemViewModel : ExtendableViewModel<IDroneFlightWidget>, IDroneFlightWidget
{
    private const string WidgetId = "plugin-widget-dashboard-item";

    public PluginFlightItemViewModel(ILoggerFactory loggerFactory, IExtensionService ext)
        : base(WidgetId, loggerFactory, ext)
    {
        Header = "Plugin Flight Item";
        Position = WorkspaceDock.Left;

        Icon = MaterialIconKind.AboutCircle;
        IconColor = AsvColorKind.Success;
    }

    public ObservableList<IDashboardWidget> DashboardWidgets { get; }

    public MaterialIconKind? Icon { get; }
    public AsvColorKind IconColor { get; }
    public string? Header { get; set; }
    public WorkspaceDock Position { get; }
    public bool IsExpanded { get; }
    public bool CanExpand { get; }
    public MenuTree? MenuView { get; }
    public bool IsVisible { get; set; }
    public int DisplayPriority { get; private set; }
    public DeviceId? DeviceId { get; private set; }
    public int SubOrder { get; private set; }

    public void Attach(DeviceId deviceId)
    {
        DeviceId = deviceId;
        DisplayPriority = deviceId.DisplayPriority;
        SubOrder = 1;

        InitArgs(deviceId.AsString());
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
