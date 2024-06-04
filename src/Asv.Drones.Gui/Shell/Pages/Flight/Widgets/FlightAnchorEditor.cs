using System.Composition;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IMapWidget))]
[method: ImportingConstructor]
public class FlightAnchorEditor(ILocalizationService loc)
    : AnchorsEditorViewModel(WellKnownUri.ShellPageMapFlightWidgetAnchorEditor, loc);