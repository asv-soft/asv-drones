using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface IMavParamPropertyViewModel : IPropertyViewModel
{
    MavParamInfo Info { get; }
}
