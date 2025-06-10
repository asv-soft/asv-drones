using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
[Shared]
public class StartMissionCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.uav.start";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_StartMission,
        Description = RS.UavAction_StartMission_Description,
        Icon = MaterialIconKind.MapMarkerPath,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        UavWidgetViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var control = context.Device.GetMicroservice<ControlClient>();
        var mission = context.Device.GetMicroservice<MissionClientEx>();

        if (control is null || mission is null)
        {
            return null;
        }

        await mission.SetCurrent(0, cancel);
        await control.SetAutoMode(cancel);
        return null;
    }
}
