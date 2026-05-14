using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record BatteryRttBoxData(
    double Charge,
    double Amperage,
    double Voltage,
    double Consumed,
    IUnitItem ProgressUnit,
    IUnitItem AmperageUnit,
    IUnitItem CapacityUnit,
    IUnitItem VoltageUnit
);
#pragma warning restore SA1313

public class BatteryUavIndicatorViewModel : KeyValueRttBoxViewModel<BatteryRttBoxData>
{
    private readonly AsvColorKind defaultStatusColor;

    [SetsRequiredMembers]
    public BatteryUavIndicatorViewModel()
        : this(
            nameof(BatteryUavIndicator),
            DesignTime.LoggerFactory,
            new ReactiveProperty<double>(0.76),
            new ReactiveProperty<double>(12.4),
            new ReactiveProperty<double>(23.8),
            new ReactiveProperty<double>(3900),
            DeviceTelemetryDesignPreview.Unit(ProgressUnit.Id).CurrentUnitItem,
            DeviceTelemetryDesignPreview.Unit(AmperageUnit.Id).CurrentUnitItem,
            DeviceTelemetryDesignPreview.Unit(CapacityUnit.Id).CurrentUnitItem,
            DeviceTelemetryDesignPreview.Unit(VoltageUnit.Id).CurrentUnitItem,
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

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
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            batteryCharge
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
            networkErrorTimeout
        )
    {
        this.defaultStatusColor = defaultStatusColor;

        Header = RS.UavRttItem_Battery;
        Icon = MaterialIconKind.Battery10;
        UpdateAction = (model, changes) =>
        {
            model[
                0,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryCharge_Header,
                changes.ProgressUnit.Symbol
            ].ValueString = changes.ProgressUnit.PrintFromSi(changes.Charge, "F2");
            model[
                1,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryAmperage_Header,
                changes.AmperageUnit.Symbol
            ].ValueString = changes.AmperageUnit.PrintFromSi(changes.Amperage, "F2");
            model[
                2,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryVoltage_Header,
                changes.VoltageUnit.Symbol
            ].ValueString = changes.VoltageUnit.PrintFromSi(changes.Voltage, "F2");
            model[
                3,
                RS.UavWidgetViewModel_BatteryRttBox_BatteryConsumed_Header,
                changes.CapacityUnit.Symbol
            ].ValueString = changes.CapacityUnit.PrintFromSi(changes.Consumed, "F2");

            ChangeBatteryStatus(changes.Charge);
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
