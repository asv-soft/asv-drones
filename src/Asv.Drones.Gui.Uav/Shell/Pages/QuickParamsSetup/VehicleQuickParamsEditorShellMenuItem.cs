using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IShellMenuItem<IVehicleClient>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class VehicleQuickParamsEditorShellMenuItem : ShellMenuItem,IShellMenuItem<IVehicleClient>
{
    public VehicleQuickParamsEditorShellMenuItem() : base($"asv:shell.menu.uav.tune?{Guid.NewGuid()}")
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.TuneVertical);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 99;
        Name = "Basic tune";
    }


    public IShellMenuItem Init(IVehicleClient target)
    {
        NavigateTo = QuickParamsSetupPageViewModel.GenerateUri(target.FullId, target.Class);
        return this;
    }
}