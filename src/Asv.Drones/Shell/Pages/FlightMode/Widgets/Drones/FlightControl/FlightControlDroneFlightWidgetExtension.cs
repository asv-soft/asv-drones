using System.Linq;
using System.Reactive.Linq;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class FlightControlDroneFlightWidgetExtension(
    IDeviceManager conn,
    INavigationService navigationService,
    IUnitService unitService,
    IMapService mapService,
    ILoggerFactory loggerFactory
) : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var flightControlViewModel = new FlightControlViewModel(
            navigationService,
            unitService,
            conn,
            loggerFactory
        );

        flightControlViewModel.Attach(context.DeviceId!);
        context.DashboardWidgets.Add(flightControlViewModel);
    }
}
