using System.Reactive.Linq;
using Asv.Common;
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
    
    public HomeDistanceUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle, GenerateRtt(vehicle,"homedistance"))
    {
        Order = 1;
        
        Vehicle.Position.HomeDistance
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => HomeDistance = localization.Distance.FromSiToStringWithUnits(_))
            .DisposeItWith(Disposable);
        
    }

    [Reactive] public string HomeDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;
}