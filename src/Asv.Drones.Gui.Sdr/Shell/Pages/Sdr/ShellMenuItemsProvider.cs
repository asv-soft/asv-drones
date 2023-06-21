using System.ComponentModel.Composition;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrShellPageProvider : ViewModelProviderBase<IShellMenuItem>
{
    
    [ImportingConstructor]
    public SdrShellPageProvider(IMavlinkDevicesService svc)
    {
        svc.Payloads.Transform(_ => (IShellMenuItem)new ShellMenuItem($"asv:shell.menu.sdr.{_.Heartbeat.FullId}")
        {
            Name = "Records store",
            NavigateTo = SdrViewModel.GenerateUri(_.Heartbeat.FullId),
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.DatabaseEye),
            Position = ShellMenuPosition.Top,
            Type = ShellMenuItemType.PageNavigation,
            Order = _.Heartbeat.FullId
        }).ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
    }
    
    
}