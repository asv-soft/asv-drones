using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

[ExportExtensionFor<IHomePageItem>]
[method: ImportingConstructor]
public class HomePageParamsDeviceItemAction(ILoggerFactory loggerFactory) : HomePageDeviceItemAction
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

        return new ActionViewModel("params", loggerFactory)
        {
            Icon = MaterialIconKind.CogTransferOutline,
            Header = RS.HomePageParamsDeviceItemAction_ActionViewModel_Header,
            Description = RS.HomePageParamsDeviceItemAction_ActionViewModel_Description,
            Command = new BindableAsyncCommand(OpenMavParamsCommand.Id, context),
            CommandParameter = new StringArg($"dev_id={device.Id}"), // TODO: replace with DevicePageViewModelMixin.CreateOpenPageArgs
        };
    }
}
