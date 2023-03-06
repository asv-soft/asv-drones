using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SettingsShellMenuItem : ShellMenuItem
    {
        public const string UriString = ShellMenuItem.UriString + ".settings";
        public static readonly Uri Uri = new(UriString);
        public SettingsShellMenuItem() : base(Uri)
        {
            Name = RS.SettingsShellMenuProvider_SettingsShellMenuProvider_Settings;
            NavigateTo = SettingsViewModel.Uri;
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Settings);
            Position = ShellMenuPosition.Bottom;
            Type = ShellMenuItemType.PageNavigation;
            Order = 0;
        }
    }
}