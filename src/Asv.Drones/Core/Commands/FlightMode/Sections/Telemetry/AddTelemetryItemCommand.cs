using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;

namespace Asv.Drones;

public class AddTelemetryItemCommand : TelemetrySectionCommandBase<StringArg, ListArg>
{
    public const string Id = $"{BaseId}.flight-mode.telemetry.add-item";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.AddTelemetryItemCommand_CommandInfo_Name,
        Description = RS.AddTelemetryItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Add,
        DefaultHotKey = null,
    };

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<CommandArg?> InternalExecute(
        ITelemetrySection context,
        StringArg arg,
        CancellationToken cancel
    )
    {
        return ExecuteWithBackup(context, () => context.TryAddItem(arg.Value));
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
