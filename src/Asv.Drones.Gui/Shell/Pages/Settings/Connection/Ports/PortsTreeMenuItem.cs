using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class PortsTreeMenuItem(
    IMavlinkDevicesService dev,
    ILocalizationService loc,
    ILogService log)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsConnectionsPortsUri)
{
    public override Uri ParentId => WellKnownUri.ShellPageSettingsConnectionsUri;
    public override string? Name => RS.ConnectionsPortsView_Header_Title;
    public override string? Description => RS.ConnectionsPortsView_Header_Description;
    public override MaterialIconKind Icon => MaterialIconKind.FormatListText;
    public override int Order => -1;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new PortBrowserViewModel(dev, log, loc);
    }
}