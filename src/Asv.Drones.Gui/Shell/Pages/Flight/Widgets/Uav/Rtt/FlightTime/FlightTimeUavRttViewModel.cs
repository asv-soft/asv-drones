using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class FlightTimeUavRttViewModel : UavRttItem
{
    public FlightTimeUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public FlightTimeUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle,
        "flighttime")
    {
        Order = 2;
        Vehicle.Position.ArmedTime
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ =>
                FlightTime = $"{localization.RelativeTime.ConvertToString(_)} {localization.RelativeTime.GetUnit(_)}")
            .DisposeItWith(Disposable);
        IsMinimizedVisible = true;
    }

    [Reactive] public string FlightTime { get; set; } = RS.UavRttItem_ValueNotAvailable;
}