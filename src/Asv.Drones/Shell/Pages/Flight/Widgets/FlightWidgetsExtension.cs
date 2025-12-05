using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

[ExportExtensionFor<IFlightMode>]
[method: ImportingConstructor]
public class FlightWidgetsExtension(
    IDeviceManager conn,
    INavigationService navigationService,
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : IExtensionFor<IFlightMode>
{
    public void Extend(IFlightMode context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(context.Widgets, TryCreateWidget, RemoveWidget)
            .DisposeItWith(contextDispose);
    }

    private UavWidgetViewModel? TryCreateWidget(IClientDevice device)
    {
        var isInit = device.State.CurrentValue == ClientDeviceState.Complete;

        if (!isInit)
        {
            return null;
        }

        if (device.GetMicroservice<IPositionClientEx>() is null)
        {
            return null;
        }

        return new UavWidgetViewModel(device, navigationService, unitService, conn, loggerFactory);
    }

    private bool RemoveWidget(IClientDevice model, UavWidgetViewModel vm)
    {
        return model.Id == vm.Device.Id;
    }
}
