using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SettingsShellMenuItem : DisposableViewModelBase, IShellMenuItem
    {
        public Uri Id { get; } = new("asv:shell.menu.settings");
        public string Name => RS.SettingsShellMenuProvider_SettingsShellMenuProvider_Settings;
        public Uri NavigateTo => SettingsViewModel.BaseUri;
        public string Icon => MaterialIconDataProvider.GetData(MaterialIconKind.Settings);
        public ShellMenuPosition Position => ShellMenuPosition.Bottom;
        public ShellMenuItemType Type => ShellMenuItemType.PageNavigation;
        public int Order => 0;
        public ReadOnlyObservableCollection<IShellMenuItem>? Items => null;
    }
}