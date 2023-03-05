using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightShellMenuItem : ShellMenuItem
    {
        public const string UriString = ShellMenuItem.UriString + ".flight";
        public static readonly Uri Uri = new(UriString);
        public FlightShellMenuItem():base(Uri)
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