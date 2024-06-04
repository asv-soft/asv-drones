using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(typeof(IShellMenuItem))]
public class LogViewerShellMenuItem : ShellMenuItem
{
    public LogViewerShellMenuItem() : base(WellKnownUri.ShellMenuLogViewer)
    {
        Name = RS.LogViewer_Name;
        NavigateTo = WellKnownUri.ShellPageLogViewerUri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Journal);
        Position = ShellMenuPosition.Bottom;
        Type = ShellMenuItemType.PageNavigation;
        Order = -3;
    }
}