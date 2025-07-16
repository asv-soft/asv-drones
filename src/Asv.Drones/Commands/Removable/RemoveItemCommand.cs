using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class RemoveItemCommand : ContextCommand<ISupportRemove>
{
    public const string Id = $"{BaseId}.remove";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.RemoveItemCommand_CommandInfo_Name,
        Description = RS.RemoveItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Delete,
        DefaultHotKey = "Shift + Delete",
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        ISupportRemove context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        // TODO: make removing items command undoable
        await context.RemoveAsync(cancel);
        return null;
    }
}
