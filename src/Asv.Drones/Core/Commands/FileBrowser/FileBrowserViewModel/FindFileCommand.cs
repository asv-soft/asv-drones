using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

public class FindFileCommand : ContextCommand<FileBrowserViewModel>
{
    public const string Id = $"{BaseId}.find_file";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.FindFileOnLocalCommand_CommandInfo_Name,
        Description = RS.FindFileOnLocalCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Magnify,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<CommandArg?> InternalExecute(
        FileBrowserViewModel context,
        CommandArg arg,
        CancellationToken cancel
    )
    {
        context.FindFileOnLocal();
        return ValueTask.FromResult<CommandArg?>(null);
    }
}
