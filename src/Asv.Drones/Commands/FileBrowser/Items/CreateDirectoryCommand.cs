using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class CreateDirectoryCommand : ContextCommand<IBrowserItemViewModel>
{
    public const string Id = $"{BaseId}.create_directory";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.CreateDirectoryCommand_CommandInfo_Name,
        Description = RS.CreateDirectoryCommand_CommandInfo_Description,
        Icon = MaterialIconKind.FolderAdd,
        DefaultHotKey = "Ctrl + N",
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IBrowserItemViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.CreateDirectoryAsync(cancel);
        return null;
    }
}
