using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapFlight, typeof(IViewModelProvider<IMapAnchor>))]
public class UavFlightAnchorsProvider : IViewModelProvider<IMapAnchor>
{
    private readonly IMavlinkDevicesService _svc;

    [ImportingConstructor]
    public UavFlightAnchorsProvider(IMavlinkDevicesService svc,
        ILocalizationService loc,
        [ImportMany] IEnumerable<IUavActionProvider> actions)
    {
        _svc = svc;
        var uav = svc.Vehicles.Transform(c => new FlightUavAnchor(c, loc, actions)).ChangeKey((k, a) => a.Id)
            .Transform(a => (IMapAnchor)a);
        Items = uav.MergeChangeSets(GetLayers());
    }

    private IEnumerable<IObservable<IChangeSet<IMapAnchor, Uri>>> GetLayers()
    {
        yield return _svc.Vehicles
            .Transform(x => new RoiAnchor(x))
            .ChangeKey((k, a) => a.Id)
            .Transform(a => (IMapAnchor)a);
        yield return _svc.Vehicles
            .Transform(c => new HomeAnchor(c))
            .ChangeKey((k, a) => a.Id)
            .Transform(a => (IMapAnchor)a);
        yield return _svc.Vehicles
            .Transform(c => new GoToAnchor(c))
            .ChangeKey((k, a) => a.Id)
            .Transform(a => (IMapAnchor)a);
        yield return _svc.Vehicles
            .Transform(c => new UavGoToPolygon(c))
            .ChangeKey((k, p) => p.Id)
            .Transform(p => (IMapAnchor)p);
        yield return _svc.Vehicles
            .Transform(c => new UavTrackPolygon(c))
            .ChangeKey((k, p) => p.Id)
            .Transform(p => (IMapAnchor)p);
        yield return _svc.AdsbDevices
            .Transform(d => new AdsbMapLayer(d))
            .DisposeMany()
            .TransformMany(l => l.Items, a => a.Id)
            .Transform(a => (IMapAnchor)a);
        yield return _svc.Vehicles
            .Transform(c => new UavFlightMissionMapLayer(c))
            .DisposeMany()
            .TransformMany(l => l.Items, a => a.Id)
            .Transform(a => (IMapAnchor)a)
            .DisposeMany();

        yield return _svc.Vehicles
            .Transform(c => (IMapAnchor)new UavFlightMissionPathPolygon(c))
            .ChangeKey((k, v) => v.Id)
            .DisposeMany();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
}