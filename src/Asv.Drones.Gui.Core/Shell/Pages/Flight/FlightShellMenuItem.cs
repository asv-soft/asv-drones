using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FlightShellMenuItem : DisposableViewModelBase, IShellMenuItem
    {
        public Uri Id { get; } = new(WellKnownUri.ShellMenuFlight);
        public string Name => RS.FlightShellMenuItem_Name;
        public Uri NavigateTo { get; } = new(WellKnownUri.ShellPageFlight);
        public string Icon => MaterialIconDataProvider.GetData(MaterialIconKind.Map);
        public ShellMenuPosition Position => ShellMenuPosition.Top;
        public ShellMenuItemType Type => ShellMenuItemType.PageNavigation;
        public int Order => short.MinValue;
        public ReadOnlyObservableCollection<IShellMenuItem>? Items => null;
    }

}