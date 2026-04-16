using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Drones;

public class PluginFlightItemViewModel : FlightWidgetViewModel<PluginFlightItemViewModel> // TODO: Remove on new FlightMode release
{
    private const string WidgetId = "test-widget";

    public PluginFlightItemViewModel(ILoggerFactory loggerFactory, IExtensionService ext)
        : base(WidgetId, loggerFactory, ext) { }

    public override int Order => 1;
}
