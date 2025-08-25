using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

/// <summary>
/// <para>Executes with:</para>
/// <para>- <c>arg[0]</c> as Old Value.</para>
/// <para>- <c>arg[1]</c> as New Value.</para>
/// </summary>
[ExportCommand]
public class CommitRenameCommand : ContextCommand<ISupportRename, ListArg>
{
    public const string Id = $"{BaseId}.rename.commit";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.RenameItemCommand_CommandInfo_Name,
        Description = RS.RenameItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Check,
        DefaultHotKey = "Enter",
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<ListArg?> InternalExecute(
        ISupportRename context,
        ListArg arg,
        CancellationToken cancel
    )
    {
        var oldPath = arg[0].AsString();
        var newPath = arg[1].AsString();
        if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath))
        {
            return null;
        }
        try
        {
            await context.RenameAsync(oldPath, newPath, cancel);
        }
        catch
        {
            return null;
        }
        return CommandArg.CreateList(
            CommandArg.CreateString(newPath),
            CommandArg.CreateString(oldPath)
        );
    }
}
