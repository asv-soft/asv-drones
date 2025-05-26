using Asv.Avalonia.GeoMap;
using Asv.IO;

namespace Asv.Drones.Api;

public interface IUavFlightWidget : IMapWidget
{
    IClientDevice Device { get; }
}