using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class UpdateParamsCommand : ContextCommand<MavParamsPageViewModel>
{
    public const string Id = $"{BaseId}.params.update";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UpdateParamsCommand_CommandInfo_Name,
        Description = RS.UpdateParamsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Refresh,
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
        context.UpdateParamsImpl();
        return default;
    }
}
