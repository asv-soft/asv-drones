using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PayloadsShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
    public PayloadsShellMenuItemProvider(IMavlinkDevicesService svc)
    {
        svc.Payloads.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.payload.{_.FullId}")
            {
                Name = $"Payload [{_.FullId}]",
                Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog),
                Position = ShellMenuPosition.Top,
                Type = ShellMenuItemType.PageNavigation,
                Order = _.FullId,
                Items = new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>
                {
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