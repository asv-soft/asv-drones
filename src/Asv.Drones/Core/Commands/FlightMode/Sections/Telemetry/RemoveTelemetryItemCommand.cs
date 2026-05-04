using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;

namespace Asv.Drones;

public class RemoveTelemetryItemCommand : TelemetrySectionCommandBase<StringArg, ListArg>
{
    public const string Id = $"{BaseId}.flight-mode.telemetry.remove-item";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.RemoveTelemetryItemCommand_CommandInfo_Name,
        Description = RS.RemoveTelemetryItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Delete,
        DefaultHotKey = null,
    };

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<CommandArg?> InternalExecute(
        ITelemetrySection context,
        StringArg arg,
        CancellationToken cancel
    )
    {
        return ExecuteWithBackup(context, () => context.TryRemoveItem(arg.Value));
    }

    public override ValueTask<CommandArg?> InternalExecute(
        ITelemetrySection context,
        ListArg arg,
        CancellationToken cancel
    )
    {
        return ExecuteWithBackup(context, () => context.TrySetItems(ReadItemIds(arg)));
    }
}
