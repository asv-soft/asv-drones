using System.Composition;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(nameof(DeviceClass.Plane), typeof(IShellMenuItem<IClientDevice>))]
[Export(nameof(DeviceClass.Copter), typeof(IShellMenuItem<IClientDevice>))]
public class VehicleFileBrowserMenuItem : ShellMenuItem, IShellMenuItem<IClientDevice>
{
    public VehicleFileBrowserMenuItem()
        : base(WellKnownUri.ShellMenuVehicleFileBrowser)
    {
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.FolderEyeOutline);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 99;
        Name = RS.VehiclesShellMenuItemProvider_ShellMenuItem_FileBrowser;
    }

    public IShellMenuItem Init(IClientDevice target)
    {
        NavigateTo = VehicleFileBrowserViewModel.GenerateUri(
            WellKnownUri.ShellPageVehicleFileBrowser,
            target.FullId,
            target.Class
        );
        return this;
    }
}
