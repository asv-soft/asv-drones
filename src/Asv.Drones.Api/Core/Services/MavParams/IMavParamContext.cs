using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavParamContext
{
    MavParamInfo Info { get; }
    IParamsClientEx Client { get; }
}

public class MavParamContext(MavParamInfo info, IParamsClientEx client) : IMavParamContext
{
    public MavParamInfo Info { get; } = info;
    public IParamsClientEx Client { get; } = client;
}
