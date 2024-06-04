using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class MavlinkIdTreeMenuItem(IMavlinkDevicesService svc)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsConnectionsIdentifyUri)
{
    public override Uri ParentId => WellKnownUri.ShellPageSettingsConnectionsUri;
    public override string? Name => RS.MavlinkIdTreeMenuItem_Name_Mavlink;
    public override string? Description => RS.MavlinkIdTreeMenuItem_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Identifier;
    public override int Order => -1;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new MavlinkIdViewModel(svc, (ISettingsPageContext)context);
    }
}