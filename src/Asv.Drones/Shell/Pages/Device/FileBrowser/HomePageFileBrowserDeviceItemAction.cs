using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class HomePageFileBrowserDeviceItemAction : HomePageDeviceItemAction
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

        return new ActionViewModel("browser")
        {
            Header = RS.OpenFileBrowserCommand_CommandInfo_Name,
            Description = RS.OpenFileBrowserCommand_CommandInfo_Description,
            Icon = FileBrowserViewModel.PageIcon,
            Command = new ReactiveCommand(
                async (_, _) =>
                    await context.GoTo(
                        new NavPath(
                            new NavId(
                                FileBrowserViewModel.PageId,
                                DevicePageViewModelMixin.CreateOpenPageArgs(device.Id)
                            )
                        )
                    )
            ),
        };
    }
}
