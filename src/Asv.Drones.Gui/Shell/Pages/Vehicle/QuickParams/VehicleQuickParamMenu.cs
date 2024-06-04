using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(nameof(DeviceClass.Plane), typeof(IShellMenuItem<IClientDevice>))]
[Export(nameof(DeviceClass.Copter), typeof(IShellMenuItem<IClientDevice>))]
public class VehicleQuickParamMenu : ShellMenuItem, IShellMenuItem<IClientDevice>
{
    public VehicleQuickParamMenu() : base(WellKnownUri.ShellMenuQuickParamsVehicle)
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.WrenchClock);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 200;
        Name = RS.VehiclesShellMenuItemProvider_ShellMenuItem_QuickSettings;
    }

    public IShellMenuItem Init(IClientDevice target)
    {
        NavigateTo = VehicleQuickParamPageViewModel.GenerateUri(WellKnownUri.ShellPageQuickParams, target.FullId, target.Class);
        return this;
    }
}