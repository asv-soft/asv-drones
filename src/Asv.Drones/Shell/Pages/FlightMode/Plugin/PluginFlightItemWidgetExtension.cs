using System;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class PluginFlightItemWidgetExtension(
    IDeviceManager conn,
    INavigationService navigationService,
    IUnitService unitService,
    ILoggerFactory loggerFactory,
    IMapService mapService,
    IExtensionService ext,
    IServiceProvider containerHost
) : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        var vm = new PluginFlightItemViewModel(loggerFactory, ext);

        byte targetSystemId = 2;
        byte targetComponentId = 1;

        vm.Header = $"Plugin payload for [{targetSystemId}.{targetComponentId}]";

        var clientIdentity = new MavlinkClientIdentity(255, 0, targetSystemId, targetComponentId);

        var mavlinkClientDeviceId = new MavlinkClientDeviceId("copter", clientIdentity);
        vm.Attach(mavlinkClientDeviceId);

        context.Widgets.Add(vm);

        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(5000);
                context.Widgets.Remove(vm);
                await Task.Delay(5000);
                context.Widgets.Add(vm);
            }
        });
    }
}
