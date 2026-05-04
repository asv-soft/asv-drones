using System;
using System.Linq;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public abstract class TelemetrySectionCommandBase<TArg1, TArg2>
    : ContextCommand<ITelemetrySection, TArg1, TArg2>
    where TArg1 : CommandArg
    where TArg2 : CommandArg
{
    protected static CommandArg CreateBackup(ITelemetrySection context)
    {
        var itemIds = context.Items.Select(i => CommandArg.CreateString(i.ItemId));
        return CommandArg.CreateList(itemIds);
    }

    protected static string[] ReadItemIds(ListArg arg)
    {
        return arg.Select(item => item.AsString()).ToArray();
    }

    protected static ValueTask<CommandArg?> ExecuteWithBackup(
        ITelemetrySection context,
        Func<bool> execute
    )
    {
        var backup = CreateBackup(context);
        var changed = execute();

        return ValueTask.FromResult(changed ? backup : null);
    }
}
