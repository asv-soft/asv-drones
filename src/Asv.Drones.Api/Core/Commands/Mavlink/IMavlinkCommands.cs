using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavlinkCommands
{
    ICommandInfo WriteParamInfo { get; }
    ValueTask WriteParam(
        IViewModel context,
        string name,
        MavParamValue value,
        CancellationToken cancel = default
    );
    ICommandInfo ReadParamInfo { get; }
    ValueTask ReadParam(IViewModel context, string name, CancellationToken cancel = default);
}
