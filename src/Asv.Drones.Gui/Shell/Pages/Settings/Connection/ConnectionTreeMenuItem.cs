using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class ConnectionTreeMenuItem(IMavlinkDevicesService svc)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsConnectionsUri)
{
    public override Uri ParentId => WellKnownUri.UndefinedUri;
    public override string? Name => RS.ConnectionsShellMenuItem_Name;
    public override string? Description => RS.ConnectionTreeMenuItem_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Lan;
    public override int Order => 200;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return null;
    }
}