using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class WriteParamCommand : ContextCommand<ParamItemViewModel, ActionArg>
{
    public const string Id = $"{BaseId}.params.item.write";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.WritePatamCommand_CommandInfo_Name,
        Description = RS.WriteParamCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Upload,
        DefaultHotKey = null, // TODO: make a key bind when new key listener system appears
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<ActionArg?> InternalExecute(
        ParamItemViewModel context,
        ActionArg arg,
        CancellationToken cancel
    )
    {
        await context.WriteImpl(cancel);
        return null;
    }
}
