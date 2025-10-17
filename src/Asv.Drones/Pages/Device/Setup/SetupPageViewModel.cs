using System.Composition;
using System.Threading;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.Drones.Api;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class SetupPageViewModelConfig { }

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
        ILoggerFactory loggerFactory
    )
        : base(PageId, devices, cmd, container, layoutService, loggerFactory)
    {
        Icon = PageIcon;
    }

    public bool IsDeviceInitialized
    {
        get;
        set => SetField(ref field, value);
    }

    protected override void AfterDeviceInitialized(
        IClientDevice device,
        CancellationToken onDisconnectedToken
    )
    {
        IsDeviceInitialized = true;
        Title = $"{RS.SetupPageViewModel_Title}[{device.Id}]";
        onDisconnectedToken.Register(() =>
        {
            IsDeviceInitialized = false;
        });
    }

    public override IExportInfo Source => SystemModule.Instance;
}
