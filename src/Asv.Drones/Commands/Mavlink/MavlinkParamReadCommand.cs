using System;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
[Shared]
public class MavlinkParamReadCommand : MavlinkMicroserviceCommand<IParamsClientEx, StringArg>
{
    public const string Id = $"{BaseId}.mavlink.param.read";
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Read mavlink param",
        Description = "Read mavlink param from device",
        Icon = MaterialIconKind.Set,
        Source = IoModule.Instance,
        DefaultHotKey = null,
    };

    public static ValueTask Execute(
        IRoutable context,
        string name,
        CancellationToken cancel = default
    )
    {
        return context.ExecuteCommand(Id, CommandArg.CreateString(name), cancel: cancel);
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<StringArg?> InternalExecute(
        IParamsClientEx microservice,
        StringArg arg,
        CancellationToken cancel
    )
    {
        await microservice.ReadOnce(arg.Value, cancel);
        return null; // this is command without undo, so we return null
    }
}
