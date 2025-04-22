using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones;

[ExportExtensionFor<IHomePageItem>]
public class HomePageParamsDeviceItemAction : HomePageDeviceItemAction
{
    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDeviceItem context
    )
    {
        if (device.GetMicroservice<IParamsClientEx>() == null)
        {
            return null;
        }

        return new ActionViewModel("params")
        {
            Icon = MaterialIconKind.CogTransferOutline,
            Header = RS.HomePageParamsDeviceItemAction_ActionViewModel_Header,
            Description = RS.HomePageParamsDeviceItemAction_ActionViewModel_Description,
            Command = new BindableAsyncCommand(OpenMavParamsCommand.Id, context),
            CommandParameter = new StringCommandArg(device.Id.AsString()),
        };
    }
}
