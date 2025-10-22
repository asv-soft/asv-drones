using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class ExportPacketsToCsvCommand : ContextCommand<PacketViewerViewModel>
{
    public const string Id = $"{BaseId}.packet-viewer.export-to-csv";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ExportPacketsToCsvCommand_CommandInfo_Name,
        Description = RS.ExportPacketsToCsvCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ContentSave,
        DefaultHotKey = null, // TODO: make a key bind later
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        PacketViewerViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.ExportToCsvImpl(cancel);
        return null;
    }
}
