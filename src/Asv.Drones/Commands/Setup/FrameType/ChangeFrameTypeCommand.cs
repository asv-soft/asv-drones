using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class ChangeFrameTypeCommand : ContextCommand<SetupFrameTypeViewModel, StringArg>
{
    public const string Id = $"{BaseId}.setup.frame-type.change";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeFrameTypeCommand_CommandInfo_Name,
        Description = RS.ChangeFrameTypeCommand_CommandInfo_Description,
        Icon = MaterialIconKind.KeyChange,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<StringArg?> InternalExecute(
        SetupFrameTypeViewModel context,
        StringArg newValue,
        CancellationToken cancel
    )
    {
        var currentFrameId = context.CurrentFrame?.Value?.Id;

        if (currentFrameId is null)
        {
            return null;
        }

        await context.ChangeFrameType(newValue.Value, cancel);

        return new StringArg(currentFrameId);
    }
}
