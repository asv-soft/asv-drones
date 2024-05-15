using System.Composition;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IMapWidget))]
[method: ImportingConstructor]
public class PlaningAnchorEditor(ILocalizationService loc)
    : AnchorsEditorViewModel(WellKnownUri.ShellPageMapPlaningWidgetAnchorEditor, loc);