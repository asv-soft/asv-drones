using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageMapPlaning, typeof(IViewModelProvider<IMapAnchor>))]
public class UavPlaningAnchorsProvider : IViewModelProvider<IMapAnchor>
{
    private readonly IMavlinkDevicesService _svc;

    [ImportingConstructor]
    public UavPlaningAnchorsProvider(IMavlinkDevicesService svc,
        ILocalizationService loc)
    {
        _svc = svc;
        Items = svc.Vehicles.Transform(c => new PlaningUavAnchor(c, loc)).ChangeKey((k, a) => a.Id)
            .Transform(a => (IMapAnchor)a);
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
}