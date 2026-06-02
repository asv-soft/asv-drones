using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

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

        Target
            .Where(w => w.HasValue)
            .Select(w => w!.Value)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(w => OnDeviceConnected(w.Device))
            .DisposeItWith(Disposable);
    }

    private void OnDeviceConnected(IClientDevice device)
    {
        Header = $"{RS.SetupPageViewModel_Title}[{device.Id}]";
        TreeHeader = RS.SetupPageViewModel_Title;
    }
}
