using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class VoltageUavRttItemViewModel : UavRttItem
{
    public VoltageUavRttItemViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Voltage = "0.7 V";
    }

    public VoltageUavRttItemViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle,
        "voltage")
    {
        Order = 5;

        Vehicle.Rtt.BatteryVoltage
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(_ => Voltage = $"{localization.Voltage.ConvertToStringWithUnits(_)}")
            .DisposeItWith(Disposable);
        
        IsMinimizedVisible = true;
    }

    [Reactive] public string Voltage { get; set; }
}