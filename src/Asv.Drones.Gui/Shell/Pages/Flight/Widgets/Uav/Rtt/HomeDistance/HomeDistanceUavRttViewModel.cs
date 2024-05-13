using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class HomeDistanceUavRttViewModel : UavRttItem
{
    public HomeDistanceUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        HomeDistance = "0.7 m";
    }

    public HomeDistanceUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle,
        "homedistance")
    {
        Order = 1;

        Vehicle.Position.HomeDistance
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => HomeDistance = localization.Distance.FromSiToStringWithUnits(_))
            .DisposeItWith(Disposable);
        
        IsMinimizedVisible = true;
    }

    [Reactive] public string HomeDistance { get; set; } = RS.UavRttItem_ValueNotAvailable;
}