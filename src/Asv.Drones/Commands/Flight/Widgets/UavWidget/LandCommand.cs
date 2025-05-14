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
public class LandCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.uav.land";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_Land,
        Description = RS.UavAction_Land_Description,
        Icon = MaterialIconKind.AeroplaneLanding,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandArg?> InternalExecute(
        UavWidgetViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var control = context.Device.GetMicroservice<ControlClient>();

        if (control == null)
        {
            return null;
        }

        await control.EnsureGuidedMode(cancel: cancel);
        await control.DoLand(cancel);
        return null;
    }
}
