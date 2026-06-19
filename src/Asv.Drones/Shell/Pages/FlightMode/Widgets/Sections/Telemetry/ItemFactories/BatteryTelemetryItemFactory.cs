using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class BatteryTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "battery";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null
        && device.GetMicroservice<ITelemetryClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();
        var telemetryClient = device.GetRequiredMicroservice<ITelemetryClientEx>();

        var batteryObservable = telemetryClient
            .BatteryCharge.Prepend(double.NaN)
            .CombineLatest(
                telemetryClient.BatteryCurrent.Prepend(double.NaN),
                telemetryClient.BatteryVoltage.Prepend(double.NaN),
                unitService.Units[ProgressUnit.Id].CurrentUnitItem,
                unitService.Units[AmperageUnit.Id].CurrentUnitItem,
                unitService.Units[CapacityUnit.Id].CurrentUnitItem,
                unitService.Units[VoltageUnit.Id].CurrentUnitItem,
                (
                    charge,
                    amperage,
                    voltage,
                    progressUnit,
                    amperageUnit,
                    capacityUnit,
                    voltageUnit
                ) =>
                {
                    var consumed = amperage.ApproximatelyEquals(0d)
                        ? double.NaN
                        : Math.Round(
                            amperage * positionClientEx.ArmedTime.CurrentValue.TotalHours,
                            2
                        );

                    return new BatteryRttBoxData(
                        charge,
                        amperage,
                        voltage,
                        consumed,
                        progressUnit,
                        amperageUnit,
                        capacityUnit,
                        voltageUnit
                    );
                }
            );

        return InternalCreate(batteryObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var batteryObservable = unitService
            .Units[ProgressUnit.Id]
            .CurrentUnitItem.CombineLatest(
                unitService.Units[AmperageUnit.Id].CurrentUnitItem,
                unitService.Units[CapacityUnit.Id].CurrentUnitItem,
                unitService.Units[VoltageUnit.Id].CurrentUnitItem,
                (progressUnit, amperageUnit, capacityUnit, voltageUnit) =>
                    new BatteryRttBoxData(
                        0.76d,
                        12.4d,
                        23.8d,
                        3.9d,
                        progressUnit,
                        amperageUnit,
                        capacityUnit,
                        voltageUnit
                    )
            );

        return InternalCreate(batteryObservable);
    }

    private IRttBoxViewModel InternalCreate(Observable<BatteryRttBoxData> observable)
    {
        return new KeyValueRttBoxViewModel<BatteryRttBoxData>(Id, loggerFactory, observable, null)
        {
            Header = RS.UavRttItem_Battery,
            Icon = MaterialIconKind.Battery10,
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

                ChangeBatteryStatus(model, changes.Charge);
            },
            Status = DefaultStatusColor,
        };
    }

    private static void ChangeBatteryStatus(
        KeyValueRttBoxViewModel<BatteryRttBoxData> rtt,
        double percent
    )
    {
        rtt.Status = percent switch
        {
            > 0.7d => DefaultStatusColor,
            > 0.5d => AsvColorKind.Warning,
            > 0.4d => AsvColorKind.Warning | AsvColorKind.Blink,
            < 0.3d => AsvColorKind.Error | AsvColorKind.Blink,
            _ => DefaultStatusColor,
        };
    }
}

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
