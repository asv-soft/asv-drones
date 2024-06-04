using System.Composition;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IMapAction))]
[method: ImportingConstructor]
public class FlightRulerAction(ILocalizationService loc)
    : MapRulerActionViewModel(WellKnownUri.ShellPageMapFlightActionRuler, loc);