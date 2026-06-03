using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class BatteryTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "battery-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Battery;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null
        && device.GetMicroservice<ITelemetryClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
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

        return new BatteryUavIndicatorViewModel(
            Id,
            loggerFactory,
            batteryObservable,
            DefaultStatusColor
        );
    }

    public ITelemetryItem CreatePreview()
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

        return new BatteryUavIndicatorViewModel(
            Id,
            loggerFactory,
            batteryObservable,
            DefaultStatusColor
        );
    }
}
