using System;
using Asv.Avalonia;
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

        var positionClientEx =
            device.GetMicroservice<IPositionClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(IPositionClientEx)} from {device.Id}"
            );
        var telemetryClient =
            device.GetMicroservice<ITelemetryClientEx>()
            ?? throw new ArgumentException(
                $"Unable to load {nameof(ITelemetryClientEx)} from {device.Id}"
            );

        var batteryAmperage = new ReactiveProperty<double>(double.NaN);
        var batteryVoltage = new ReactiveProperty<double>(double.NaN);
        var batteryCharge = new ReactiveProperty<double>(double.NaN);
        var batteryConsumed = new ReactiveProperty<double>(double.NaN);

        var indicator = new BatteryUavIndicatorViewModel(
            Id,
            loggerFactory,
            batteryCharge,
            batteryAmperage,
            batteryVoltage,
            batteryConsumed,
            unitService.Units[ProgressUnit.Id].CurrentUnitItem,
            unitService.Units[AmperageUnit.Id].CurrentUnitItem,
            unitService.Units[CapacityUnit.Id].CurrentUnitItem,
            unitService.Units[VoltageUnit.Id].CurrentUnitItem,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            batteryAmperage,
            batteryVoltage,
            batteryCharge,
            batteryConsumed,
            telemetryClient
                .BatteryCurrent.ObserveOnUIThreadDispatcher()
                .Subscribe(x => batteryAmperage.Value = x),
            telemetryClient
                .BatteryVoltage.ObserveOnUIThreadDispatcher()
                .Subscribe(x => batteryVoltage.Value = x),
            telemetryClient
                .BatteryCharge.ObserveOnUIThreadDispatcher()
                .Subscribe(x => batteryCharge.Value = x),
            telemetryClient
                .BatteryCurrent.ObserveOnUIThreadDispatcher()
                .Select(d =>
                    d == 0
                        ? double.NaN
                        : Math.Round(d * positionClientEx.ArmedTime.CurrentValue.TotalHours, 2)
                )
                .Subscribe(x => batteryConsumed.Value = x)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        var batteryCharge = new ReactiveProperty<double>(0.76);
        var batteryAmperage = new ReactiveProperty<double>(12.4);
        var batteryVoltage = new ReactiveProperty<double>(23.8);
        var batteryConsumed = new ReactiveProperty<double>(3.9);

        var indicator = new BatteryUavIndicatorViewModel(
            Id,
            loggerFactory,
            batteryCharge,
            batteryAmperage,
            batteryVoltage,
            batteryConsumed,
            unitService.Units[ProgressUnit.Id].CurrentUnitItem,
            unitService.Units[AmperageUnit.Id].CurrentUnitItem,
            unitService.Units[CapacityUnit.Id].CurrentUnitItem,
            unitService.Units[VoltageUnit.Id].CurrentUnitItem,
            DefaultStatusColor
        );

        return new TelemetryItemViewModel(
            Id,
            indicator,
            loggerFactory,
            batteryCharge,
            batteryAmperage,
            batteryVoltage,
            batteryConsumed
        );
    }
}
