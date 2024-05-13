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
    public override string? Name => "Market";
    public override string? Description => "Plugins market";
    public override MaterialIconKind Icon => MaterialIconKind.StoreSearch;
    public override int Order => 5;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new PluginsMarketViewModel(mng, log);
    }
}