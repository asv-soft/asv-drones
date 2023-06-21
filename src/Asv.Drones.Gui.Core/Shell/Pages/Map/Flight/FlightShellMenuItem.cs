using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightShellMenuItem : ShellMenuItem
    {
        public FlightShellMenuItem():base("asv:shell.menu.flight")
        {
            Name = RS.FlightShellMenuItem_Name;
            NavigateTo = FlightPageViewModel.Uri;
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Map);
            Position = ShellMenuPosition.Top;
            Type = ShellMenuItemType.PageNavigation;
            Order = short.MinValue;
        }
       
    }

}