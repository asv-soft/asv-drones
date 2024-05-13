using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class ConsumedEnergyMAhUavRttViewModel : UavRttItem
{
    public ConsumedEnergyMAhUavRttViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        Consume = "2456 mAh";
    }

    public ConsumedEnergyMAhUavRttViewModel(IVehicleClient vehicle, ILocalizationService localization) : base(vehicle,
        "consume")
    {
        Order = 15;
        
        vehicle.Rtt.BatteryCurrent.Subscribe(_ => Current = _)
            .DisposeItWith(Disposable);

        vehicle.Position.ArmedTime.Subscribe(_ =>
            {
                if (Current == 0 || _.Milliseconds == 0) return;
                Consume = $"{localization.MAh.ConvertToStringWithUnits(Current * _.TotalHours)}";
            })
            .DisposeItWith(Disposable);

        IsMinimizedVisible = true;
    }

    [Reactive] public string Consume { get; set; } = RS.UavRttItem_ValueNotAvailable;
    [Reactive] public double Current { get; set; } 
}