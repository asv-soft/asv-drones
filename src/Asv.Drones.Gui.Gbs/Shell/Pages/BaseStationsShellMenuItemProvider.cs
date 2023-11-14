using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;

namespace Asv.Drones.Gui.Gbs;


[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehiclesShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
    public VehiclesShellMenuItemProvider(IMavlinkDevicesService svc, CompositionContainer container)
    {
        svc.BaseStations.Transform(v=>(IShellMenuItem)new GbsGroupShellMenuItem(v,container))
            .ChangeKey((_, v) => v.Id)
            .DisposeMany()
            .PopulateInto(Source)
            .DisposeItWith(Disposable);
         
    }
}


public class GbsGroupShellMenuItem:ShellMenuItem
{
    public GbsGroupShellMenuItem(IGbsClientDevice gbs, CompositionContainer compositionContainer) : base($"asv:shell.menu.gbs.{gbs.FullId}")
    {
        gbs.Name.Subscribe(x => Name = x).DisposeItWith(Disposable);

        Icon = MaterialIconDataProvider.GetData(GbsIconHelper.DefaultIcon);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.Group;
        Order = gbs.FullId;

        Items = new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>(compositionContainer.GetExportedValues<IShellMenuItem<IGbsClientDevice>>()
            .Select(x=>x.Init(gbs)).OrderBy(x=>x.Order)));
        InfoBadge = new InfoBadge
        {
            Value = gbs.Identity.TargetSystemId,
        };
            
    }
}


[Export(typeof(IShellMenuItem<IGbsClientDevice>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehicleParamsEditorShellMenuItem : ShellMenuItem,IShellMenuItem<IGbsClientDevice>
{
    public VehicleParamsEditorShellMenuItem() : base($"asv:shell.menu.gbs.params?{Guid.NewGuid()}")
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 100;
        Name = "Settings";
       
    }


    public IShellMenuItem Init(IGbsClientDevice target)
    {
        NavigateTo = ParamPageViewModel.GenerateUri(target.FullId, target.Class);
        return this;
    }
}
