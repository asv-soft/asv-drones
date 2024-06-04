using System;
using System.Collections.ObjectModel;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui;

public class AdsbMapLayer : DisposableReactiveObject
{
    private readonly ReadOnlyObservableCollection<AdsbAnchor> _items;

    public AdsbMapLayer(IAdsbClientDevice device)
    {
        device.Adsb.Targets
            .Transform(v => new AdsbAnchor(device, v))
            .DisposeMany()
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
    }

    public ReadOnlyObservableCollection<AdsbAnchor> Items => _items;
}