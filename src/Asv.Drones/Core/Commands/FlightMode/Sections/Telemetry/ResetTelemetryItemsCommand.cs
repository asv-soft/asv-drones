using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;

namespace Asv.Drones;

public class ResetTelemetryItemsCommand : TelemetrySectionCommandBase<EmptyArg, ListArg>
{
    public const string Id = $"{BaseId}.flight-mode.telemetry.reset-items";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ResetTelemetryItemsCommand_CommandInfo_Name,
        Description = RS.ResetTelemetryItemsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Restore,
        DefaultHotKey = null,
    };

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<CommandArg?> InternalExecute(
        ITelemetrySection context,
        EmptyArg arg,
        CancellationToken cancel
    )
    {
        return ExecuteWithBackup(context, context.TryResetItems);
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
