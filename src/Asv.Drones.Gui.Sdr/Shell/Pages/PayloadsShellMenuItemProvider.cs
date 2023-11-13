using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PayloadsShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
    public PayloadsShellMenuItemProvider(IMavlinkDevicesService svc)
    {
        svc.Payloads.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.payload.{_.FullId}")
            {
                Name = $"SDR payload [{_.FullId}]",
                Icon = MaterialIconDataProvider.GetData(SdrIconHelper.DefaultIcon),
                Position = ShellMenuPosition.Top,
                Type = ShellMenuItemType.Group,
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