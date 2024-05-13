using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class PluginsTreeMenuItem(IMavlinkDevicesService svc)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsPluginsUri)
{
    public override Uri ParentId => WellKnownUri.UndefinedUri;
    public override string? Name => RS.PluginsTreeMenuItem_Name_Plugins;
    public override string? Description => RS.PluginsTreeMenuItem_Description;
    public override MaterialIconKind Icon => MaterialIconKind.Plugin;
    public override int Order => 300;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return null;
    }
}