using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
[Shared]
public class UpdateMissionCommand : ContextCommand<MissionProgressViewModel>
{
    #region StaticInfo

    public const string Id = $"{BaseId}.mission-items.update";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_Land,
        Description = RS.UavAction_Land_Description,
        Icon = MaterialIconKind.Reload,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

   
    protected override async ValueTask<CommandArg?> InternalExecute(MissionProgressViewModel context, CommandArg newValue, CancellationToken cancel)
    {
        await context.InitiateMissionPoints(cancel);
        return null;
    }
}
