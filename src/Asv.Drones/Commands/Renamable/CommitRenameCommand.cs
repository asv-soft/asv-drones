using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

/// <summary>
/// <para>Executes with:</para>
/// <para>- <c>arg["old"]</c> as Old Value.</para>
/// <para>- <c>arg["new"]</c> as New Value.</para>
/// </summary>
[ExportCommand]
public class CommitRenameCommand : ContextCommand<ISupportRename, DictArg>
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

    public override async ValueTask<DictArg?> InternalExecute(
        ISupportRename context,
        DictArg arg,
        CancellationToken cancel
    )
    {
        var oldValue = arg["old"].AsString();
        var newValue = arg["new"].AsString();
        if (string.IsNullOrWhiteSpace(oldValue) || string.IsNullOrWhiteSpace(newValue))
        {
            return null;
        }
        try
        {
            await context.RenameAsync(oldValue, newValue, cancel);
        }
        catch
        {
            return null;
        }
        return CommandArg.CreateDictionary(
            new Dictionary<string, CommandArg>
            {
                { "new", CommandArg.CreateString(oldValue) },
                { "old", CommandArg.CreateString(newValue) },
            }
        );
    }
}
