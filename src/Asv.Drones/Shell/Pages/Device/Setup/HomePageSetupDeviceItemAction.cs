using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class HomePageSetupDeviceItemAction : HomePageDeviceItemAction
{
    public const string StaticId = "ext.home.device-action.setup";

    public override string Id => StaticId;

    protected override IActionViewModel? TryCreateAction(
        IClientDevice device,
        HomePageDeviceItem context
    )
    {
        return new ActionViewModel(SetupPageViewModel.PageId)
        {
            Header = RS.OpenSetupCommand_CommandInfo_Name,
            Description = RS.OpenSetupCommand_CommandInfo_Description,
            Icon = SetupPageViewModel.PageIcon,
            Command = new ReactiveCommand(
                async (_, cancel) =>
                    await context.GoTo(
                        new NavPath(
                            new NavId(
                                SetupPageViewModel.PageId,
                                DevicePageViewModelMixin.CreateOpenPageArgs(device.Id)
                            )
                        ),
                        cancel
                    )
            ),
        };
    }
}
