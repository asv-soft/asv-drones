using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;

namespace Asv.Drones;

/// <summary>
/// <para>Executes with:</para>
/// <para>- <c>arg["old"]</c> as Old Value.</para>
/// <para>- <c>arg["new"]</c> as New Value.</para>
/// </summary>
[ExportCommand]
public class CommitRenameCommand : ContextCommand<ISupportRename, DictArg>, ICommitRenameCommand
{
    public const string Id = ICommitRenameCommand.CommandId;

    public const string OldValue = "old";
    public const string NewValue = "new";

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

    public override async ValueTask<DictArg?> InternalExecute(
        ISupportRename context,
        DictArg arg,
        CancellationToken cancel
    )
    {
        arg.TryGetValue(OldValue, out var oldValue);
        arg.TryGetValue(NewValue, out var newValue);

        if (oldValue is not StringArg || newValue is not StringArg)
        {
            return null;
        }

        var oldString = oldValue.AsString();
        var newString = newValue.AsString();

        if (string.IsNullOrWhiteSpace(oldString) || string.IsNullOrWhiteSpace(newString))
        {
            return null;
        }
        try
        {
            await context.RenameAsync(oldString, newString, cancel);
        }
        catch
        {
            return null;
        }
        return CommandArg.CreateDictionary(
            new Dictionary<string, CommandArg>
            {
                { NewValue, CommandArg.CreateString(oldString) },
                { OldValue, CommandArg.CreateString(newString) },
            }
        );
    }
}
