using System.Composition;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IMapAction))]
[method: ImportingConstructor]
public class PlaningZoomAction() : MapZoomActionViewModel(WellKnownUri.ShellPageMapPlaningActionZoom);