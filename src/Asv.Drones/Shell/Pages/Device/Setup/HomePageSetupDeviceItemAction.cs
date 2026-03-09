using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class HomePageSetupDeviceItemAction(ILoggerFactory loggerFactory) : HomePageDeviceItemAction
{
    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDeviceItem context
    )
    {
        return new ActionViewModel(SetupPageViewModel.PageId, loggerFactory)
        {
            Header = OpenSetupCommand.StaticInfo.Name,
            Description = OpenSetupCommand.StaticInfo.Description,
            Icon = OpenSetupCommand.StaticInfo.Icon,
            Command = new BindableAsyncCommand(OpenSetupCommand.Id, context),
            CommandParameter = DevicePageViewModelMixin.CreateOpenPageArgs(device.Id),
        };
    }
}
