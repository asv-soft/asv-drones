using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class FlightTimeUavRttViewModel : UavRttItem
{
    public FlightTimeUavRttViewModel()
    {
        
    }
    
    public FlightTimeUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle, GenerateRtt(vehicle,"flighttime"))
    {
        Order = 2;
        Vehicle.Position.ArmedTime
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => FlightTime = $"{localization.RelativeTime.ConvertToString(_)} {localization.RelativeTime.GetUnit(_)}")
            .DisposeItWith(Disposable);
    }

    [Reactive] public string FlightTime { get; set; } = RS.UavRttItem_ValueNotAvailable;
}