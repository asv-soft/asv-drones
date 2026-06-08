using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavParamsEditorFactory
{
    IEnumerable<IMavParamPropertyViewModel> CreateList(
        IParamsClientEx client,
        params IEnumerable<IMavParamTypeMetadata> paramList
    );
    IMavParamPropertyViewModel? Create(IMavParamTypeMetadata param, IParamsClientEx client);
}
