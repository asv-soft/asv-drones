using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class CalculateCrc32Command : ContextCommand<IBrowserItemViewModel>
{
    public const string Id = $"{BaseId}.crc32";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.CalculateCrc32Command_CommandInfo_Name,
        Description = RS.CalculateCrc32Command_CommandInfo_Description,
        Icon = MaterialIconKind.KeyOutline,
        DefaultHotKey = "Ctrl + Q",
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IBrowserItemViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.CalculateCrc32Async(cancel);
        return null;
    }
}
