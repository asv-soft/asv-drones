using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningShellMenuItem : ShellMenuItem
    {
        public PlaningShellMenuItem():base("asv:shell.menu.planning")
        {
            Name = RS.PlaningShellMenuItem_Name;
            NavigateTo = PlaningPageViewModel.Uri;
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.MapMarkerPath);
            Position = ShellMenuPosition.Top;
            Type = ShellMenuItemType.PageNavigation;
            Order = 0;
            
        }
       
    }

}