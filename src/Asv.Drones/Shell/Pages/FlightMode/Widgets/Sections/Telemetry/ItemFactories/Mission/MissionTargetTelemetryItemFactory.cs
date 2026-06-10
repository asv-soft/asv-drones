using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class MissionTargetTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "mission-target-distance";

    public string ItemId => Id;
    public string DisplayName => RS.MissionTargetTelemetry_DispayName;
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null
        && device.GetMicroservice<IMissionClientEx>() is not null
        && device.GetMicroservice<IGnssClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();
        var missionClient = device.GetRequiredMicroservice<IMissionClientEx>();
        var gnssClient = device.GetRequiredMicroservice<IGnssClientEx>();

        return CreateItem(
            positionClient.TargetDistance.Prepend(double.NaN),
            missionClient.Current.Prepend((ushort)0),
            gnssClient.Main.GroundVelocity.Prepend(double.NaN)
        );
    }

    public ITelemetryItem CreatePreview()
    {
        return CreateItem(
            Observable.Return(100d).Concat(Observable.Never<double>()),
            Observable.Return((ushort)3).Concat(Observable.Never<ushort>()),
            Observable.Return(10d).Concat(Observable.Never<double>())
        );
    }

    private MissionTargetTelemetryItemViewModel CreateItem(
        Observable<double> distance,
        Observable<ushort> targetIndex,
        Observable<double> groundVelocity
    )
    {
        var timeSpanUnit = unitService.Units[TimeSpanUnit.Id].AvailableUnits[
            TimeSpanHourMinuteSecondUnitItem.Id
        ];

        var targetData = distance.CombineLatest(
            targetIndex,
            groundVelocity,
            unitService.Units[DistanceUnit.Id].CurrentUnitItem,
            (targetDistance, index, velocity, unit) =>
                new TargetDistanceRttBoxData(
                    targetDistance,
                    index,
                    targetDistance / velocity,
                    unit,
                    timeSpanUnit
                )
        );

        return new MissionTargetTelemetryItemViewModel(
            Id,
            loggerFactory,
            targetData,
            DefaultStatusColor
        );
    }
}
