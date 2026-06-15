using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Avalonia.Threading;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class MissionLayer : DisposableOnce, IMissionLayer
{
    private readonly ObservableList<IDeviceMissionLayer> _devices = [];
    private readonly CompositeDisposable _disposable = [];

    public MissionLayer(
        ICollection<IMapAnchor> mapAnchors,
        IDeviceManager deviceManager,
        IExtensionService ext
    )
    {
        _devices.DisposeRemovedItems().DisposeItWith(_disposable);
        _disposable.AddAction(() => _devices.ClearWithItemsDispose());

        deviceManager
            .Explorer.InitializedDevices.PopulateTo(
                _devices,
                TryCreateMission,
                IsMissionLayer,
                synchronizationContext: new AvaloniaSynchronizationContext(
                    Dispatcher.UIThread,
                    DispatcherPriority.Default
                )
            )
            .DisposeItWith(_disposable);
        return;

        DeviceMissionLayer? TryCreateMission(IClientDevice device)
        {
            var mission = device.GetMicroservice<IMissionClientEx>();
            if (mission is null)
            {
                return null;
            }

            return new DeviceMissionLayer(
                mapAnchors,
                device,
                mission,
                deviceManager.GetDeviceColor(device.Id),
                ext
            );
        }
    }

    public IReadOnlyObservableList<IDeviceMissionLayer> Devices => _devices;

    public bool TryFind(DeviceId deviceId, out IDeviceMissionLayer? layer)
    {
        layer = _devices.FirstOrDefault(device => device.DeviceId == deviceId);
        return layer is not null;
    }

    public IDeviceMissionLayer? FindOrDefault(DeviceId deviceId)
    {
        return _devices.FirstOrDefault(device => device.DeviceId == deviceId);
    }

    public IDeviceMissionLayer Find(DeviceId deviceId)
    {
        return _devices.First(device => device.DeviceId == deviceId);
    }

    protected override void InternalDisposeOnce()
    {
        _disposable.Dispose();
    }

    private static bool IsMissionLayer(IClientDevice device, IDeviceMissionLayer layer)
    {
        return layer.DeviceId == device.Id;
    }
}
