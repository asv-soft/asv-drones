using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class MissionProgressTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "mission-progress";

    public string ItemId => Id;
    public string DisplayName => RS.MissionProgressTelemetry_DisplayName;
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IMissionClientEx>() is not null
        && device.GetMicroservice<IPositionClientEx>() is not null
        && device.GetMicroservice<IModeClient>() is not null
        && device.GetMicroservice<IGnssClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        IClientDevice clientDevice = device;
        var missionClient = device.GetRequiredMicroservice<IMissionClientEx>();
        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();
        var modeClient = device.GetRequiredMicroservice<IModeClient>();
        var gnssClient = device.GetRequiredMicroservice<IGnssClientEx>();
        var progressData = Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Select(_ =>
                CalculateProgressData(
                    clientDevice,
                    missionClient,
                    positionClient,
                    modeClient,
                    gnssClient
                )
            )
            .Prepend(new MissionProgressData(double.NaN, double.NaN, double.NaN));

        return CreateItem(progressData);
    }

    public ITelemetryItem CreatePreview()
    {
        return CreateItem(
            Observable
                .Return(new MissionProgressData(900d, 0.7d, 900d))
                .Concat(Observable.Never<MissionProgressData>())
        );
    }

    private MissionProgressTelemetryItemViewModel CreateItem(
        Observable<MissionProgressData> progressData
    )
    {
        var timeSpanUnit = unitService.Units[TimeSpanUnit.Id].AvailableUnits[
            TimeSpanHourMinuteSecondUnitItem.Id
        ];
        var remainingData = progressData.CombineLatest(
            unitService.Units[DistanceUnit.Id].CurrentUnitItem,
            (progress, unit) =>
                new MissionProgressRttBoxData(
                    progress.RemainingDistance,
                    progress.Progress,
                    progress.RemainingTime,
                    unit,
                    timeSpanUnit
                )
        );

        return new MissionProgressTelemetryItemViewModel(
            Id,
            loggerFactory,
            remainingData,
            DefaultStatusColor
        );
    }

    private static MissionProgressData CalculateProgressData(
        IClientDevice device,
        IMissionClientEx missionClient,
        IPositionClientEx positionClient,
        IModeClient modeClient,
        IGnssClientEx gnssClient
    )
    {
        if (!IsOnMission(device, missionClient, positionClient, modeClient, gnssClient))
        {
            return new MissionProgressData(double.NaN, double.NaN, double.NaN);
        }

        var missionDistance = missionClient.AllMissionsDistance.CurrentValue * 1000;
        var currentIndex = missionClient.Current.CurrentValue;
        var passedDistance = CalculatePassedDistance(missionClient, currentIndex);
        var toTargetDistance = GeoMath.Distance(
            positionClient.Target.CurrentValue,
            positionClient.Current.CurrentValue
        );
        var remainingDistance = Math.Abs(missionDistance - passedDistance + toTargetDistance);
        var remainingTime = remainingDistance / gnssClient.Main.GroundVelocity.CurrentValue;

        var progress = CalculatePathProgressValue(
            device,
            missionClient,
            missionDistance,
            remainingDistance,
            currentIndex
        );

        return new MissionProgressData(
            remainingDistance,
            Math.Clamp(progress, 0, 1),
            remainingTime
        );
    }

    private static double CalculatePassedDistance(
        IMissionClientEx missionClient,
        ushort currentIndex
    )
    {
        var items = missionClient
            .MissionItems.Where(item =>
                item.Index <= currentIndex && item.Command.Value != MavCmd.MavCmdDoChangeSpeed
            )
            .ToList();

        if (items.Count < 2)
        {
            return 0;
        }

        double passedDistance = 0;
        for (var i = 1; i < items.Count; i++)
        {
            passedDistance += GeoMath.Distance(
                items[i - 1].Location.Value,
                items[i].Location.Value
            );
        }

        return passedDistance;
    }

    private static double CalculatePathProgressValue(
        IClientDevice device,
        IMissionClientEx missionClient,
        double missionDistance,
        double remainingDistance,
        ushort currentIndex
    )
    {
        switch (device)
        {
            case ArduCopterClientDevice:
                if (missionClient.Reached.CurrentValue > 0)
                {
                    return Math.Abs((missionDistance - remainingDistance) / missionDistance);
                }

                if (currentIndex == missionClient.MissionItems.Count)
                {
                    return 1;
                }

                return 0;
            default:
                return Math.Abs((missionDistance - remainingDistance) / missionDistance);
        }
    }

    private static bool IsOnMission(
        IClientDevice device,
        IMissionClientEx missionClient,
        IPositionClientEx positionClient,
        IModeClient modeClient,
        IGnssClientEx gnssClient
    )
    {
        var currentMode = modeClient.CurrentMode.CurrentValue;
        var isAutoMode = device switch
        {
            ArduCopterClientDevice => currentMode == ArduCopterMode.Auto,
            ArduPlaneClientDevice => currentMode == ArduPlaneMode.Auto,
            _ => currentMode == ArduCopterMode.Auto || currentMode == ArduPlaneMode.Auto,
        };
        if (!isAutoMode)
        {
            return false;
        }

        if (!missionClient.IsSynced.CurrentValue)
        {
            return false;
        }

        if (missionClient.MissionItems.Count == 0)
        {
            return false;
        }

        if (
            missionClient.MissionItems.All(item =>
                item.Command.Value < MavCmd.MavCmdNavWaypoint
                || item.Command.Value > MavCmd.MavCmdNavLast
            )
        )
        {
            return false;
        }

        var missionDistance = missionClient.AllMissionsDistance.CurrentValue;
        if (!double.IsFinite(missionDistance) || missionDistance <= 0)
        {
            return false;
        }

        var currentIndex = missionClient.Current.CurrentValue;
        var firstMissionIndex = missionClient.MissionItems.Min(item => item.Index);
        var lastMissionIndex = missionClient.MissionItems.Max(item => item.Index);
        if (currentIndex < firstMissionIndex || currentIndex > lastMissionIndex)
        {
            return false;
        }

        var targetDistance = GeoMath.Distance(
            positionClient.Target.CurrentValue,
            positionClient.Current.CurrentValue
        );
        if (!double.IsFinite(targetDistance))
        {
            return false;
        }

        var groundVelocity = gnssClient.Main.GroundVelocity.CurrentValue;
        return double.IsFinite(groundVelocity);
    }

    private sealed record MissionProgressData(
        double RemainingDistance,
        double Progress,
        double RemainingTime
    );
}
