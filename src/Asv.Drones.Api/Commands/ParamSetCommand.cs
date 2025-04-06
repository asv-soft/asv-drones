using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.AsvRsga;
using Material.Icons;

namespace Asv.Drones.Api;

[Export(typeof(IAsyncCommand))]
[Shared]
[method:ImportingConstructor]
public class WriteMavParamCommand : ContextCommand<IDevicePage>
{
    public const string Id = $"{BaseId}.mavlink.params.write";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Write mavlink PARAMS",
        Description = "Write mavlink PARAMS",
        Icon = MaterialIconKind.Upload,
        Source = SystemModule.Instance,
        DefaultHotKey = null,
    };
    
   
    public static ICommandArg CreateArg(AsvRsgaCustomMode mode)
    {
        return CommandArg.FromChangeAction(string.Empty, mode.ToString("G"));
    }
    
    public static bool TryGetArg(ICommandArg commandArg, out AsvRsgaCustomMode mode)
    {
        mode = AsvRsgaCustomMode.AsvRsgaCustomModeIdle;
        return commandArg is ActionCommandArg action && Enum.TryParse(action.Value, out mode);
    }
    
    public override ICommandInfo Info => StaticInfo;
    
    protected override ValueTask<ICommandArg?> InternalExecute(IDevicePage context, ICommandArg newValue, CancellationToken cancel)
    {
        var microservice = context.Device?.GetMicroservice<IParamsClientEx>();
        /*if (microservice == null)
        {
            throw new Exception($"Error to execute command {nameof(RsgaSetModeCommand)}[{Id}]: device microservice {nameof(IAsvRsgaClientEx)} not found");
        }

        if (TryGetArg(newValue, out var mode) == false)
        {
            throw new Exception($"Error to execute command {nameof(RsgaSetModeCommand)}[{Id}]: invalid command argument '{newValue}'");
        }

        var rollback = CreateArg(microservice.CurrentMode.CurrentValue);
        await microservice.SetMode(mode, cancel);
        return rollback;*/
        return ValueTask.FromResult<ICommandArg?>(null);
    }
}