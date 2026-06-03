using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

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
            Icon = MavParamsPageViewModel.PageIcon,
            Header = RS.OpenMavParamsCommand_CommandInfo_Name,
            Description = RS.OpenMavParamsCommand_CommandInfo_Description,
            Command = new ReactiveCommand(
                async (_, _) =>
                    await context.GoTo(
                        new NavPath(
                            new NavId(
                                MavParamsPageViewModel.PageId,
                                DevicePageViewModelMixin.CreateOpenPageArgs(device.Id)
                            )
                        )
                    )
            ),
        };
    }
}
