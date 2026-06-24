using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Avalonia.Threading;
using R3;

namespace Asv.Drones;

public class FlightModeMissionAnchorsExtension(IDeviceManager manager, IExtensionService ext)
    : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
        manager
            .Explorer.InitializedDevices.PopulateTo(
                context.Map.Anchors,
                TryCreateMissionContainerAnchor,
                IsMissionContainerAnchor,
                synchronizationContext: new AvaloniaSynchronizationContext(
                    Dispatcher.UIThread,
                    DispatcherPriority.Default
                )
            )
            .DisposeItWith(contextDispose);
        return;

        IMissionContainerAnchor? TryCreateMissionContainerAnchor(IClientDevice device)
        {
            var mission = device.GetMicroservice<IMissionClientEx>();
            if (mission is null)
            {
                return null;
            }

            return new MissionContainerAnchor(
                context.Map.Anchors,
                device,
                manager.GetDeviceColor(device.Id),
                ext
            );
        }
    }

    private static bool IsMissionContainerAnchor(
        IClientDevice device,
        IMissionContainerAnchor anchor
    )
    {
        return anchor.DeviceId == device.Id;
    }
}
