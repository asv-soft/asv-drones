using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class FlightModeClientDeviceWidgetExtension(
    IDeviceManager conn,
    IClientDeviceWidgetFactory factory
) : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(
                context.Widgets,
                TryCreateWidget,
                IsRemoveWidget
            )
            .DisposeItWith(contextDispose);
    }

    private IFlightWidget? TryCreateWidget(IClientDevice device)
    {
        return factory.CreateWidget(in device);
    }

    private static bool IsRemoveWidget(IClientDevice model, IFlightWidget vm)
    {
        return model.Id.ToString() == vm.Id.Args;
    }
}
