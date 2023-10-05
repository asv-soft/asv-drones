using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ParametersShellPageProvider : ViewModelProviderBase<IShellMenuItem>
{
    
    [ImportingConstructor]
    public ParametersShellPageProvider(IMavlinkDevicesService svc)
    {
        svc.Vehicles.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.params.{_.FullId}")
        {
            Name = string.Format(RS.ParametersEditorPageViewModel_Title, _.FullId),
            NavigateTo = ParamPageViewModel.GenerateUri(_.FullId, _.Class),
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog),
            Position = ShellMenuPosition.Top,
            Type = ShellMenuItemType.PageNavigation,
            Order = _.FullId
        }).ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
        svc.Payloads.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.params.{((IClientDevice)_).FullId}")
            {
                Name = string.Format(RS.ParametersEditorPageViewModel_Title, ((IClientDevice)_).FullId),
                NavigateTo = ParamPageViewModel.GenerateUri(((IClientDevice)_).FullId, ((IClientDevice)_).Class),
                Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog),
                Position = ShellMenuPosition.Top,
                Type = ShellMenuItemType.PageNavigation,
                Order = ((IClientDevice)_).FullId
            }).ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
        svc.BaseStations.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.params.{_.FullId}")
            {
                Name = string.Format(RS.ParametersEditorPageViewModel_Title, _.FullId),
                NavigateTo = ParamPageViewModel.GenerateUri(_.FullId, _.Class),
                Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog),
                Position = ShellMenuPosition.Top,
                Type = ShellMenuItemType.PageNavigation,
                Order = _.FullId
            }).ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
    }
    
    
}