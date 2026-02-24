// TODO: asv-soft-u08

using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public record BatteryRttBoxData(
    double charge,
    double amperage,
    double voltage,
    double consumed,
    IUnitItem progressUnit,
    IUnitItem amperageUnit,
    IUnitItem capacityUnit,
    IUnitItem voltageUnit
);

public class BatteryUavIndicatorViewModel : KeyValueRttBoxViewModel<BatteryRttBoxData>
{
    private readonly AsvColorKind defaultStatusColor;
    
    [SetsRequiredMembers]
    public BatteryUavIndicatorViewModel(
        NavigationId id, 
        ILoggerFactory loggerFactory, 
        ReactiveProperty<double> batteryCharge,
        ReactiveProperty<double> batteryAmperage,
        ReactiveProperty<double> batteryVoltage,
        ReactiveProperty<double> batteryConsumed,
        SynchronizedReactiveProperty<IUnitItem> progressUnit,
        SynchronizedReactiveProperty<IUnitItem> amperageUnit,
        SynchronizedReactiveProperty<IUnitItem> capacityUnit,
        SynchronizedReactiveProperty<IUnitItem> voltageUnit,
        AsvColorKind defaultStatusColor) 
        : base(id, loggerFactory,  batteryCharge
                .CombineLatest(
                    batteryAmperage,
                    batteryVoltage,
                    batteryConsumed,
                    progressUnit,
                    amperageUnit,
                    capacityUnit,
                    voltageUnit,
                    (bC, bA, bV, bCo, prog, amp, cap, vol) =>
                        new BatteryRttBoxData(bC, bA, bV, bCo, prog, amp, cap, vol)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            null)
    {
        this.defaultStatusColor = defaultStatusColor;
        
        Header = RS.UavRttItem_Battery;
        Icon = MaterialIconKind.Battery10;
        UpdateAction = (model, changes) =>
        {
            model[
                0,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryCharge_Header,
                changes.progressUnit.Symbol
            ].ValueString = changes.progressUnit.PrintFromSi(changes.charge, "F2");
            model[
                1,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryAmperage_Header,
                changes.amperageUnit.Symbol
            ].ValueString = changes.amperageUnit.PrintFromSi(changes.amperage, "F2");
            model[
                2,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryVoltage_Header,
                changes.voltageUnit.Symbol
            ].ValueString = changes.voltageUnit.PrintFromSi(changes.voltage, "F2");
            model[
                3,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryConsumed_Header,
                changes.capacityUnit.Symbol
            ].ValueString = changes.capacityUnit.PrintFromSi(changes.consumed, "F2");

            ChangeBatteryStatus(changes.charge);
        };
        Status = defaultStatusColor;
    }
    
    private void ChangeBatteryStatus(double percent)
    {
        Status = percent switch
        {
            > 0.7d => defaultStatusColor,
            > 0.5d => AsvColorKind.Warning,
            > 0.4d => AsvColorKind.Warning | AsvColorKind.Blink,
            < 0.3d => AsvColorKind.Error | AsvColorKind.Blink,
            _ => defaultStatusColor,
        };
    }
}