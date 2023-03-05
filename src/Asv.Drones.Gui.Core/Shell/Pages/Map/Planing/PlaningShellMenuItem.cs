using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningShellMenuItem : ShellMenuItem
    {
        public const string UriString = ShellMenuItem.UriString + ".planing";
        public static readonly Uri Uri = new(UriString);
        
        public PlaningShellMenuItem():base(Uri)
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