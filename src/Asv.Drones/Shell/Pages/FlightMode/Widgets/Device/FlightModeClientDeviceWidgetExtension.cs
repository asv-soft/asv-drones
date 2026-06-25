using System.Linq;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Modeling;
using Avalonia.Threading;
using R3;

namespace Asv.Drones;

public class FlightModeClientDeviceWidgetExtension(
    IDeviceManager conn,
    IClientDeviceWidgetFactory factory
) : IExtensionFor<IFlightModePage>
{
    public const string StaticId = "ext.flight-mode.client-device-widget";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        conn.Explorer.InitializedDevices.PopulateTo(
                context.Widgets,
                TryCreateWidget,
                IsRemoveWidget,
                synchronizationContext: new AvaloniaSynchronizationContext(
                    Dispatcher.UIThread,
                    DispatcherPriority.Default
                )
            )
            .DisposeItWith(contextDispose);
    }

    private IFlightWidget? TryCreateWidget(IClientDevice device)
    {
        return factory.CreateWidget(in device);
    }

    private static bool IsRemoveWidget(IClientDevice model, IFlightWidget vm)
    {
        var widgetDeviceId = vm
            .Id.Args.FirstOrDefault(x => x.Key == DevicePageViewModelMixin.ArgsDeviceIdKey)
            .Value;
        return widgetDeviceId == model.Id.AsString();
    }
}
