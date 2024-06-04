using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class CurrentUavRttViewModel : UavRttItem
{
    public CurrentUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Current = "0.7 A";
    }

    public CurrentUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle, "current")
    {
        Order = 4;
        Vehicle.Rtt.BatteryCurrent
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => Current = $"{localization.Current.ConvertToStringWithUnits(_)}")
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }

    [Reactive] public string Current { get; set; }
}