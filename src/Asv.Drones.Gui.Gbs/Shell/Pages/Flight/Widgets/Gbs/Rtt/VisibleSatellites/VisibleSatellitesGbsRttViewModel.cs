using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class VisibleSatellitesGbsRttViewModel : GbsRttItem
{
    public VisibleSatellitesGbsRttViewModel()
    {
        
    }

    public VisibleSatellitesGbsRttViewModel(IGbsClientDevice baseStation) : base(baseStation, GenerateUri(baseStation,"visiblesatellites"))
    {
        Order = 1;
        
        BaseStation.Gbs.AllSatellites
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => VisibleSatellites = _.ToString())
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }
    
    [Reactive]
    public string VisibleSatellites { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}