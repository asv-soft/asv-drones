using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class TelemetryDroneFlightWidgetExtension(
    ILoggerFactory loggerFactory,
    IDeviceManager deviceManager,
    IUnitService unitService,
    IMapService mapService
) : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var deviceId = context.DeviceId;
        if (deviceId is null)
        {
            return;
        }

        var vm = new TelemetryViewModel(deviceManager, loggerFactory, unitService);
        vm.Attach(context.DeviceId!);
        context.DashboardWidgets.Add(vm);
    }
}
