using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(typeof(IShellMenuItem))]
public class SettingsShellMenuItem : ShellMenuItem
{
    public SettingsShellMenuItem() : base(WellKnownUri.ShellMenuSettingsUri)
    {
        Name = RS.SettingsShellMenuProvider_SettingsShellMenuProvider_Settings;
        NavigateTo = WellKnownUri.ShellPageSettingsUri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Settings);
        Position = ShellMenuPosition.Bottom;
        Type = ShellMenuItemType.PageNavigation;
        Order = 0;
    }
}