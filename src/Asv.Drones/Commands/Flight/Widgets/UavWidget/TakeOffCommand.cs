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
public class TakeOffCommand : ContextCommand<UavWidgetViewModel, DoubleArg>
{
    #region Static

    public const string Id = $"{BaseId}.uav.takeOff";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_TakeOff,
        Description = RS.UavAction_TakeOff_Description,
        Icon = MaterialIconKind.AeroplaneTakeoff,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<DoubleArg?> InternalExecute(UavWidgetViewModel context, DoubleArg arg, CancellationToken cancel)
    {
        var device = context.Device;
        var controlClient = device.GetMicroservice<ControlClient>();

        if (controlClient == null)
        {
            return null;
        }

        await controlClient.SetGuidedMode(cancel);
        await controlClient.TakeOff(arg.Value, cancel);
        return null;
    }
}
