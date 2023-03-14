using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class HomeDistanceUavRttViewModel : UavRttItem
{
    
    public HomeDistanceUavRttViewModel()
    {
        HomeDistance = "0.7 m";
    }
    
    public HomeDistanceUavRttViewModel(IVehicle vehicle, ILocalizationService localization) : base(vehicle, GenerateRtt(vehicle,"homedistance"))
    {
        Order = 1;
        
        Vehicle.HomeDistance
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => HomeDistance = localization.Distance.FromSIToStringWithUnits(_.Value))
            .DisposeWith(Disposable);
        
    }
    [Reactive]
    public string HomeDistance { get; set; }
}