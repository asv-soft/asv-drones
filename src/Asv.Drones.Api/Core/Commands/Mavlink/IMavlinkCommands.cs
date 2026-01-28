using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavlinkCommands
{
    ICommandInfo WriteParamInfo { get; }
    ValueTask WriteParam(
        IRoutable context,
        string name,
        MavParamValue value,
        CancellationToken cancel = default
    );
    ICommandInfo ReadParamInfo { get; }
    ValueTask ReadParam(IRoutable context, string name, CancellationToken cancel = default);
}
