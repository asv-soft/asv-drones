using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones.Api;


[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class MavlinkParamsWriteCommand(ILoggerFactory loggerFactory) : ContextCommand<IDevicePage, ActionArg>
{
    private ILogger<MavlinkParamsWriteCommand> _logger = loggerFactory.CreateLogger<MavlinkParamsWriteCommand>();
    
    public const string Id = $"{BaseId}.params.write";
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Write mavlink param",
        Description = "Write mavlink param to device",
        Icon = MaterialIconKind.Set,
        Source = IoModule.Instance,
        DefaultHotKey = null,
    };

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<ActionArg?> InternalExecute(IDevicePage context, ActionArg arg, CancellationToken cancel)
    {
        return null;
        /*var ifc = context.Device?.GetMicroservice<IParamsClientEx>();
        if (ifc == null)
        {
            return null;
        }



        if (string.IsNullOrWhiteSpace(arg.SubjectId))
        {
            throw new ArgumentException($"{nameof(arg.SubjectId)} cannot be null or empty.", nameof(arg.SubjectId));
        }

        if (arg.Value is not { TypeId: CommandArg.Id.String })
        {
            throw new ArgumentException(
                $"{nameof(arg.Value)} must be of type {CommandArg.Id.String}.",
                nameof(arg.Value)
            );
        }

        MavParamValue prevValue;
        if (ifc.Items.TryGetValue(arg.SubjectId, out var param) == false)
        {
            prevValue = await ifc.ReadOnce(arg.SubjectId, CancellationToken.None);
        }
        else
        {
            prevValue = param.Value.Value;
        }

        if (MavParamValue.TryParseValue(arg.Value, prevValue.Type, out var value) == false)
        {

        }


        return CommandArg.ChangeAction(arg.SubjectId, CommandArg.FromString(prevValue.PrintValue()));*/


    }
}

