using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehiclesShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
     public VehiclesShellMenuItemProvider(IMavlinkDevicesService svc)
     {
         svc.Vehicles.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.uav.{_.FullId}")
         {
             Name = $"Arducopter [{_.FullId}]",
             Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Navigation),
             Position = ShellMenuPosition.Top,
             Type = ShellMenuItemType.Group,
             Order = _.FullId,
             Items = new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>()
             {
                 new ShellMenuItem(new Uri($"{QuickParamsSetupPageViewModel.UriString}.{_.FullId}"))
                 {
                     Name = string.Format("Quick params setup [{0}]", _.FullId),
                     NavigateTo = QuickParamsSetupPageViewModel.GenerateUri(_.FullId, _.Class),
                     Icon = MaterialIconDataProvider.GetData(MaterialIconKind.GearPlay),
                     Position = ShellMenuPosition.Top,
                     Type = ShellMenuItemType.PageNavigation,
                     Order = _.FullId
                 },
                 new ShellMenuItem($"asv:shell.menu.params.{_.FullId}")
                 {
                     Name = "Settings",
                     NavigateTo = ParamPageViewModel.GenerateUri(_.FullId, _.Class),
                     Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog),
                     Position = ShellMenuPosition.Top,
                     Type = ShellMenuItemType.PageNavigation,
                     Order = _.FullId
                 }
             })
         }).ChangeKey((_, v) => v.Id)
             .DisposeMany()
             .PopulateInto(Source)
             .DisposeItWith(Disposable);
     }
}