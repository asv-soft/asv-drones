using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class BatteryUavRttViewModel : UavRttItem
{
    public BatteryUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        BatteryLevel = 0.7;
        BatteryLevelString = "0.7";
    }

    public BatteryUavRttViewModel(IVehicleClient vehicle) : base(vehicle, "battery")
    {
        Vehicle.Rtt.BatteryCharge
            .Subscribe(_ => BatteryLevel = _)
            .DisposeItWith(Disposable);

        Vehicle.Rtt.BatteryCharge
            .Subscribe(_ => BatteryLevelString = _.ToString("P0"))
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }

    [Reactive] public double BatteryLevel { get; set; }

    [Reactive] public string BatteryLevelString { get; set; } = RS.UavRttItem_ValueNotAvailable;
}