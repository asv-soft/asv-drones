using Asv.Avalonia.GeoMap;
using Asv.IO;

namespace Asv.Drones.Api;

public interface IDeviceAnchor : IMapAnchor
{
    public IClientDevice Device { get; }
}
