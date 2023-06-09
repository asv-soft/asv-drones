using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class AdsbMapLayer : DisposableViewModelBase
{
    private readonly ReadOnlyObservableCollection<AdsbAnchor> _items;

    public AdsbMapLayer(IAdsbClientDevice device,ILocalizationService loc)
    {
        device.Adsb.Targets
            .Transform(_ => new AdsbAnchor(_,loc,device.FullId))
            .ObserveOn(RxApp.MainThreadScheduler)
            .DisposeMany()
            .Bind(out _items)
            .Subscribe()
            .DisposeItWith(Disposable);
    }
    
    public ReadOnlyObservableCollection<AdsbAnchor> Items => _items;
}