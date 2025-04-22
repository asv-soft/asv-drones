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
public class AutoModeCommand : ContextCommand<UavWidgetViewModel> // TODO: make basic class for commands that change the uav mode
{
    #region Static

    public const string Id = $"{BaseId}.change.mode.auto";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_AutoMode_Name,
        Description = RS.UavAction_AutoMode_Description,
        Icon = MaterialIconKind.Automatic,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        UavWidgetViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var control = context.Device.GetMicroservice<ControlClient>();
        control?.SetAutoMode(cancel);
        return default;
    }
}
