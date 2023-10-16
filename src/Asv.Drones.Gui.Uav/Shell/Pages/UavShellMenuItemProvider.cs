using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class UavShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
     public UavShellMenuItemProvider(IMavlinkDevicesService svc)
     {
         svc.Vehicles.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.uav.{_.FullId}")
         {
             Name = $"Arducopter [{_.FullId}]",
             Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Drone),
             Position = ShellMenuPosition.Top,
             Type = ShellMenuItemType.PageNavigation,
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
                     Name = string.Format(RS.ParametersEditorPageViewModel_Title, _.FullId),
                     NavigateTo = ParamPageViewModel.GenerateUri(_.FullId, _.Class),
                     Icon = MaterialIconDataProvider.GetData(MaterialIconKind.ViewList),
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