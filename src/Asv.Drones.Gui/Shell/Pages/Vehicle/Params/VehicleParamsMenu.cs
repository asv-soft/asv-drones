using System.Composition;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(nameof(DeviceClass.Plane), typeof(IShellMenuItem<IClientDevice>))]
[Export(nameof(DeviceClass.Copter), typeof(IShellMenuItem<IClientDevice>))]
public class VehicleParamsMenu : ShellMenuItem, IShellMenuItem<IClientDevice>
{
    public VehicleParamsMenu() : base(WellKnownUri.ShellMenuParamsVehicle)
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchCog);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 100;
        Name = RS.VehiclesShellMenuItemProvider_ShellMenuItem_Settings;
    }

    public IShellMenuItem Init(IClientDevice target)
    {
        NavigateTo = ParamPageViewModel.GenerateUri(WellKnownUri.ShellPageParamsVehicle, target.FullId, target.Class);
        return this;
    }
}