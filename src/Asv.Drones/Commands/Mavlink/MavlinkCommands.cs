using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;

namespace Asv.Drones;

public class MavlinkCommands : IMavlinkCommands
{
    public ICommandInfo WriteParamInfo => MavlinkParamsWriteCommand.StaticInfo;

    public ValueTask WriteParam(
        IRoutable context,
        string name,
        MavParamValue value,
        CancellationToken cancel = default
    ) => MavlinkParamsWriteCommand.Execute(context, name, value, cancel);

    public ICommandInfo ReadParamInfo => MavlinkParamReadCommand.StaticInfo;

    public ValueTask ReadParam(
        IRoutable context,
        string name,
        CancellationToken cancel = default
    ) => MavlinkParamReadCommand.Execute(context, name, cancel);
}
