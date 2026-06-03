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
        IPageContext context,
        IDeviceManager devices,
        IServiceProvider container,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, devices, container, loggerFactory, dialogService, ext)
    {
        Icon = PageIcon;
    }
}
