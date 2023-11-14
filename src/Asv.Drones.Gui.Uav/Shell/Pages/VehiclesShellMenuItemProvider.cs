using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using MavlinkHelper = Asv.Drones.Gui.Core.MavlinkHelper;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IViewModelProvider<IShellMenuItem>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehiclesShellMenuItemProvider : ViewModelProviderBase<IShellMenuItem>
{
    [ImportingConstructor]
     public VehiclesShellMenuItemProvider(IMavlinkDevicesService svc, CompositionContainer container)
     {
         svc.Vehicles.Transform(v=>(IShellMenuItem)new VehicleGroupShellMenuItem(v,container))
                .ChangeKey((_, v) => v.Id)
                .DisposeMany()
                .PopulateInto(Source)
                .DisposeItWith(Disposable);
         
     }
}

public class VehicleGroupShellMenuItem:ShellMenuItem
{
    public VehicleGroupShellMenuItem(IVehicleClient vehicle, CompositionContainer compositionContainer) : base($"asv:shell.menu.uav.{vehicle.FullId}")
    {
        vehicle.Name.Subscribe(x => Name = x).DisposeItWith(Disposable);

        Icon = MaterialIconDataProvider.GetData(MavlinkHelper.GetIcon(vehicle.Class));
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.Group;
        Order = vehicle.FullId;

        Items = new ReadOnlyObservableCollection<IShellMenuItem>(new ObservableCollection<IShellMenuItem>(compositionContainer.GetExportedValues<IShellMenuItem<IVehicleClient>>()
            .Select(x=>x.Init(vehicle)).OrderBy(x=>x.Order)));
        InfoBadge = new InfoBadge
        {
            Value = vehicle.Identity.TargetSystemId,
        };
            
    }
}

[Export(typeof(IShellMenuItem<IVehicleClient>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehicleParamsEditorShellMenuItem : ShellMenuItem,IShellMenuItem<IVehicleClient>
{
    public VehicleParamsEditorShellMenuItem() : base($"asv:shell.menu.uav.params?{Guid.NewGuid()}")
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 100;
        Name = "Settings";
    }


    public IShellMenuItem Init(IVehicleClient target)
    {
        NavigateTo = ParamPageViewModel.GenerateUri(target.FullId, target.Class);
        return this;
    }
}