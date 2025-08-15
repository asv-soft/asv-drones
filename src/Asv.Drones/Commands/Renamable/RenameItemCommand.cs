using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class RenameItemCommand : ContextCommand<IRenamable, ActionArg>
{
    public const string Id = $"{BaseId}.rename";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.RenameItemCommand_CommandInfo_Name,
        Description = RS.RenameItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Pencil,
        DefaultHotKey = HotKeyInfo.Parse("Enter"),
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<ActionArg?> InternalExecute(
        IRenamable context,
        ActionArg arg,
        CancellationToken cancel
    )
    {
        var oldPath = arg.SubjectId;
        var newName = arg.Value?.AsString();
        if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newName))
        {
            return null;
        }

        var oldName = PathEx.NameOf(oldPath);
        string newPath;
        try
        {
            newPath = await context.RenameItemAsync(oldPath, newName, cancel);
        }
        catch
        {
            return null;
        }
        return CommandArg.ChangeAction(newPath, CommandArg.CreateString(oldName));
    }
}
