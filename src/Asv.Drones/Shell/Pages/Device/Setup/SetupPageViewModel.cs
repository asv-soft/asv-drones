using System;
using System.Threading;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class SetupPageViewModel : TreeDevicePageViewModel<ISetupPage, ISetupSubpage>, ISetupPage
{
    public const string PageId = "setup";
    public const MaterialIconKind PageIcon = MaterialIconKind.Cogs;

    public SetupPageViewModel(
        ICommandService cmd,
        IDeviceManager devices,
        IServiceProvider container,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, devices, cmd, container, layoutService, loggerFactory, dialogService, ext)
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
}
