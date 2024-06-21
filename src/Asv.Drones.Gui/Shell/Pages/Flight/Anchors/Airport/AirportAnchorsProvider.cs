using System;
using System.Composition;
using Asv.Drones.Gui.Airports;
using Asv.Drones.Gui.Anchors;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapAnchor>))]
[Export(WellKnownUri.ShellPageMapPlaning, typeof(IViewModelProvider<IMapAnchor>))]
public class AirportAnchorsProvider : IViewModelProvider<IMapAnchor>
{
    
    [ImportingConstructor]
    public AirportAnchorsProvider(IApplicationHost applicationHost)
    {
        // TODO applicationHost.Paths.AppDataFolder
        
        AirportsLoader.Load(
            "C:\\Users\\etogood\\Desktop\\repos\\asv-drones\\src\\Asv.Drones.Gui.Api\\Tools\\Controls\\Map\\Resources\\airports.csv",
            [
                AirportType.SeaplaneBase
            ]);
        
        Items = AirportsLoader.Airports
            .Transform(p => new AirportAnchor(p.Id) {Location = p.Location})
            .ChangeKey((a, k) => k.Id)
            .Transform(a => (IMapAnchor)a);
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
}