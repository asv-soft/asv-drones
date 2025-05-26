using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class StopUpdateParamsCommand : ContextCommand<MavParamsPageViewModel>
{
    public const string Id = $"{BaseId}.params.stop-update";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.StopUpdateParamsCommand_CommandInfo_Name,
        Description = RS.StopUpdateParamsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.CancelCircle,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null }, // TODO: make a key bind when new key listener system appears
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        MavParamsPageViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        context.StopUpdateParamsImpl();
        return default;
    }
}
