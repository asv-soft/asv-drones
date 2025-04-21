using Asv.Avalonia.Map;
using Asv.IO;

namespace Asv.Drones.Api;

public interface IUavFlightWidget : IMapWidget
{
    IClientDevice Device { get; }
}
