using System.Composition;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IMapAction))]
[method: ImportingConstructor]
public class PlaningRulerAction(ILocalizationService loc)
    : MapRulerActionViewModel(WellKnownUri.ShellPageMapPlaningActionRuler, loc);