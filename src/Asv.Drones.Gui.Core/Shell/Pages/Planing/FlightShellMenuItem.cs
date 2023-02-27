using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningShellMenuItem : DisposableViewModelBase, IShellMenuItem
    {
        public Uri Id => WellKnownUri.ShellMenuPlaningUri;
        public string Name => RS.PlaningShellMenuItem_Name;
        public Uri NavigateTo => WellKnownUri.ShellPagePlaningUri;
        public string Icon => MaterialIconDataProvider.GetData(MaterialIconKind.MapMarkerPath);
        public ShellMenuPosition Position => ShellMenuPosition.Top;
        public ShellMenuItemType Type => ShellMenuItemType.PageNavigation;
        public int Order => 0;
        public ReadOnlyObservableCollection<IShellMenuItem>? Items => null;
    }

}