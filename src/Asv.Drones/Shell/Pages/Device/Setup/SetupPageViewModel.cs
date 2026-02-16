using System.Composition;
using System.Threading;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

[ExportPage(PageId)]
public class SetupPageViewModel : TreeDevicePageViewModel<ISetupPage, ISetupSubpage>, ISetupPage
{
    public const string PageId = "setup";
    public const MaterialIconKind PageIcon = MaterialIconKind.Cogs;

    [ImportingConstructor]
    public SetupPageViewModel(
        ICommandService cmd,
        IDeviceManager devices,
        IContainerHost container,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, devices, cmd, container, layoutService, loggerFactory, dialogService)
    {
        Icon = PageIcon;
    }

    protected override void AfterDeviceInitialized(
        IClientDevice device,
        CancellationToken onDisconnectedToken
    )
    {
        Title = $"{RS.SetupPageViewModel_Title}[{device.Id}]";
        TreeHeader = RS.SetupPageViewModel_Title;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
