using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

[ExportExtensionFor<IHomePageItem>]
[method: ImportingConstructor]
public class HomePageFileBrowserExtension(ILoggerFactory loggerFactory) : HomePageDeviceItemAction
{
    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDeviceItem context
    )
    {
        if (device.GetMicroservice<IFtpClient>() == null)
        {
            return null;
        }

        return new ActionViewModel("browser", loggerFactory)
        {
            Header = OpenFileBrowserCommand.StaticInfo.Name,
            Description = OpenFileBrowserCommand.StaticInfo.Description,
            Icon = OpenFileBrowserCommand.StaticInfo.Icon,
            Command = new BindableAsyncCommand(OpenFileBrowserCommand.Id, context),
            CommandParameter = new StringArg($"dev_id={device.Id}"), // TODO: replate with DevicePageViewModelMixin.CreateOpenPageArgs
        };
    }
}
