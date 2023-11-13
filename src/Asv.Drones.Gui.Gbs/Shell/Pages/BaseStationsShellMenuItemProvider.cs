using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Gbs;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class BaseStationsShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
    public BaseStationsShellMenuItemProvider(IMavlinkDevicesService svc)
    {
        svc.BaseStations.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.base-station.{_.FullId}")
            {
                Name = $"Ground station [{_.FullId}]",
                Icon = MaterialIconDataProvider.GetData(GbsIconHelper.DefaultIcon),
                Position = ShellMenuPosition.Top,
                Type = ShellMenuItemType.PageNavigation,
                Order = _.FullId,
                Items = new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>
                {
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