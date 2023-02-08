using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningShellMenuItem : DisposableViewModelBase, IShellMenuItem
    {
        public Uri Id { get; } = new("asv:shell.menu.planing");
        public string Name => RS.PlaningShellMenuItem_Name;
        public Uri NavigateTo => PlaningViewModel.BaseUri;
        public string Icon => MaterialIconDataProvider.GetData(MaterialIconKind.MapMarkerPath);
        public ShellMenuPosition Position => ShellMenuPosition.Top;
        public ShellMenuItemType Type => ShellMenuItemType.PageNavigation;
        public int Order => 0;
        public ReadOnlyObservableCollection<IShellMenuItem>? Items => null;
    }

}