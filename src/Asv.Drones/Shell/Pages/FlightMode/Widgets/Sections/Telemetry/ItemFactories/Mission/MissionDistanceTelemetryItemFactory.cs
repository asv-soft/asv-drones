using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class MissionDistanceTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "mission-distance";

    public string ItemId => Id;
    public string DisplayName => RS.MissionDistanceTelemetry_DisplayName;
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IMissionClientEx>() is not null
        && device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var missionClient = device.GetRequiredMicroservice<IMissionClientEx>();
        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();
        var distance = missionClient.AllMissionsDistance.CombineLatest(
            missionClient.MissionItems.ObserveChanged(),
            positionClient.Home,
            (distance, _, _) => distance
        );

        var totalDistance = distance
            .Select(d => CalculateTotalDistance(d, missionClient, positionClient))
            .Prepend(double.NaN);
        var missionDistance = distance
            .Select(_ => missionClient.AllMissionsDistance.CurrentValue * 1000)
            .Prepend(double.NaN);

        return CreateItem(totalDistance, missionDistance);
    }

    public ITelemetryItem CreatePreview()
    {
        return CreateItem(
            Observable.Return(1100d).Concat(Observable.Never<double>()),
            Observable.Return(1000d).Concat(Observable.Never<double>())
        );
    }

    private MissionDistanceTelemetryItemViewModel CreateItem(
        Observable<double> totalDistance,
        Observable<double> missionDistance
    )
    {
        var distanceData = totalDistance.CombineLatest(
            missionDistance,
            unitService.Units[DistanceUnit.Id].CurrentUnitItem,
            (total, mission, unit) => new MissionDistanceRttBoxData(total, mission, unit)
        );

        return new MissionDistanceTelemetryItemViewModel(
            Id,
            loggerFactory,
            distanceData,
            DefaultStatusColor
        );
    }

    private static double CalculateTotalDistance(
        double missionDistanceKm,
        IMissionClientEx missionClient,
        IPositionClientEx positionClient
    )
    {
        var missionDistance = missionDistanceKm * 1000;
        var start = missionClient.MissionItems.FirstOrDefault();
        var stop = missionClient.MissionItems.LastOrDefault(missionItem =>
            missionItem.Command.Value != MavCmd.MavCmdNavReturnToLaunch
        );

        if (start is null || stop is null || positionClient.Home.CurrentValue is null)
        {
            return missionDistance;
        }

        var home = positionClient.Home.CurrentValue.Value;

        return missionDistance
            + GeoMath.Distance(start.Location.Value, home)
            + GeoMath.Distance(stop.Location.Value, home);
    }
}
