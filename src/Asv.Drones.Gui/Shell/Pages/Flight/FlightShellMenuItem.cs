using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui
{
    [Export(typeof(IShellMenuItem))]
    public class FlightShellMenuItem : ShellMenuItem
    {
        public FlightShellMenuItem() : base(WellKnownUri.ShellMenuMapFlightUri)
        {
            Name = RS.FlightShellMenuItem_Name;
            NavigateTo = WellKnownUri.ShellPageMapFlightUri;
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Map);
            Position = ShellMenuPosition.Top;
            Type = ShellMenuItemType.PageNavigation;
            Order = short.MinValue;
        }
    }
}