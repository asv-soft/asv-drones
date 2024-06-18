using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class AppearanceTreeMenuItem(IApplicationHost host, ILocalizationService loc, IMapService map, IPluginManager pluginManager)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsAppearanceUri)
{
    public override Uri ParentId => WellKnownUri.UndefinedUri;
    public override string? Name => "Appearance";
    public override string? Description => "Appearance e.g. theme, language, versions, licence...";
    public override MaterialIconKind Icon => MaterialIconKind.Information;
    public override int Order => 100;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new AppearanceViewModel((ISettingsPageContext)context, host, loc, map, pluginManager);
    }
}