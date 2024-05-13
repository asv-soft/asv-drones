using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class PluginsInstalledTreeMenuItem(IPluginManager mng, ILogService log)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsPluginsLocalUri)
{
    public override Uri ParentId => WellKnownUri.ShellPageSettingsPluginsUri;
    public override string? Name => RS.PluginsInstalledTreeMenuItem_Name;
    public override string? Description => RS.PluginsInstalledTreeMenuItem_Description;
    public override MaterialIconKind Icon => MaterialIconKind.LocalActivity;
    public override int Order => 5;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new PluginsInstalledViewModel(mng, log, context);
    }
}