using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavlinkCommands
{
    ICommandInfo WriteParamInfo { get; }
    ValueTask WriteParam(IRoutable context, string name, MavParamValue value);
    ICommandInfo ReadParamInfo { get; }
    ValueTask UpdateParam(IRoutable context, string name);
}