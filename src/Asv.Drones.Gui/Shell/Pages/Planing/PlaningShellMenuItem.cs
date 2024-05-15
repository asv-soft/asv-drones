using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui
{
    [Export(typeof(IShellMenuItem))]
    public class PlaningShellMenuItem : ShellMenuItem
    {
        public PlaningShellMenuItem() : base(WellKnownUri.ShellMenuMapPlaningUri)
        {
            Name = RS.PlaningShellMenuItem_Name;
            NavigateTo = WellKnownUri.ShellPageMapPlaningUri;
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.MapMarkerPath);
            Position = ShellMenuPosition.Top;
            Type = ShellMenuItemType.PageNavigation;
            Order = 0;
        }
    }
}