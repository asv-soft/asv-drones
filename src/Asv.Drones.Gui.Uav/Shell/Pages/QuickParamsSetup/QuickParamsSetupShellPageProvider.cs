using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class QuickParamsSetupShellPageProvider : ViewModelProviderBase<IShellMenuItem>
{
    public const string UriString = "asv:shell.page.quickparamssetup";

    [ImportingConstructor]
    public QuickParamsSetupShellPageProvider(IMavlinkDevicesService svc)
    {
        svc.Vehicles.Transform(_ => (IShellMenuItem)new ShellMenuItem(new Uri($"{QuickParamsSetupPageViewModel.UriString}.{_.FullId}"))
            {
                Name = string.Format("Quick params setup [{0}]", _.FullId),
                NavigateTo = QuickParamsSetupPageViewModel.GenerateUri(_.FullId, _.Class),
                Icon = MaterialIconDataProvider.GetData(MaterialIconKind.GearPlay),
                Position = ShellMenuPosition.Top,
                Type = ShellMenuItemType.PageNavigation,
                Order = _.FullId
            }).ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
    }
}