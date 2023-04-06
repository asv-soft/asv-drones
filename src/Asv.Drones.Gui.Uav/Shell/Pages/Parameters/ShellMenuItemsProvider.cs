using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Avalonia.Controls.Mixins;
using DynamicData;
using Material.Icons;
using Disposable = System.Reactive.Disposables.Disposable;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ParametersShellPageProvider : ViewModelProviderBase<IShellMenuItem>
{
    
    [ImportingConstructor]
    public ParametersShellPageProvider(IMavlinkDevicesService svc)
    {
        svc.Vehicles.Transform(_ => (IShellMenuItem)new ShellMenuItem(new($"{ShellMenuItem.UriString}.parameters.{_.FullId}"))
        {
            Name = $"Parameters editor [{_.FullId}]",
            NavigateTo = new($"{ShellPage.UriString}.parameters?Id={_.FullId}"),
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.ViewList),
            Position = ShellMenuPosition.Top,
            Type = ShellMenuItemType.PageNavigation,
            Order = _.FullId
        }).ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
    }
}