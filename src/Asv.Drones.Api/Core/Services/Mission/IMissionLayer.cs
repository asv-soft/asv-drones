using Asv.IO;
using ObservableCollections;

namespace Asv.Drones.Api;

public interface IMissionLayer : IDisposable
{
    IReadOnlyObservableList<IDeviceMissionLayer> Devices { get; }

    public bool TryFind(DeviceId deviceId, out IDeviceMissionLayer? layer);

    public IDeviceMissionLayer? FindOrDefault(DeviceId deviceId);

    IDeviceMissionLayer Find(DeviceId deviceId);
}
