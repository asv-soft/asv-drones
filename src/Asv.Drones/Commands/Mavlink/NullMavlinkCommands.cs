using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;

namespace Asv.Drones;

public sealed class NullMavlinkCommands : IMavlinkCommands
{
    public static NullMavlinkCommands Instance { get; } = new();

    private NullMavlinkCommands() { }

    public ICommandInfo WriteParamInfo => MavlinkParamsWriteCommand.StaticInfo;

    public ValueTask WriteParam(
        IRoutable context,
        string name,
        MavParamValue value,
        CancellationToken cancel = default
    )
    {
        return ValueTask.CompletedTask;
    }

    public ICommandInfo ReadParamInfo => MavlinkParamReadCommand.StaticInfo;

    public ValueTask ReadParam(IRoutable context, string name, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }
}
