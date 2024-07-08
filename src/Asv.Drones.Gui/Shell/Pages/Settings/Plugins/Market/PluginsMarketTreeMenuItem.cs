using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class PluginsMarketTreeMenuItem(IPluginManager mng, ILogService log)
    : TreePageMenuItem($"{WellKnownUri.ShellPageSettingsPluginsUri}.market")
{
    public override Uri ParentId => WellKnownUri.ShellPageSettingsPluginsUri;
    public override string? Name => RS.PluginsMarketTreeMenuItem_Name;
    public override string? Description => RS.PluginsMarketTreeMenuItem_Description;
    public override MaterialIconKind Icon => MaterialIconKind.StoreSearch;
    public override int Order => 5;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new PluginsMarketViewModel(mng, log);
    }
}