using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavParamsEditorFactory
{
    IMavParamPropertyViewModel Create(IMavParamTypeMetadata param, IParamsClientEx client);
}

public interface IMavParamPropertyViewModel : IPropertyViewModel
{
    MavParamInfo Info { get; }
}
