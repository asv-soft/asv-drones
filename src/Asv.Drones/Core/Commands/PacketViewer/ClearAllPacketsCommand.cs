using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public sealed class ClearAllPacketsCommand : ContextCommand<PacketViewerViewModel>
{
    public const string Id = $"{BaseId}.packet-viewer.clear-all";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ClearAllPacketsCommand_CommandInfo_Name,
        Description = RS.ClearAllPacketsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Bin,
        DefaultHotKey = null, // TODO: make a key bind later
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<CommandArg?> InternalExecute(
        PacketViewerViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        context.ClearAllImpl();
        return ValueTask.FromResult<CommandArg?>(null);
    }
}
