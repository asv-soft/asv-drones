using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class MissionTotalDistanceTelemetryItemFactory(
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : ITelemetryItemFactory
{
    public const string Id = "mission-distance";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IMissionClientEx>() is not null
        && device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
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
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[DistanceUnit.Id].CurrentUnitItem,
                (total, unit) => new MissionDistanceTelemetryData(total, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<MissionDistanceTelemetryData>(Id, totalDistance, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.MissionTotalDistanceTelemetry_Header,
            ShortHeader = RS.MissionTotalDistanceTelemetry_Header_Short,
            Icon = MaterialIconKind.LocationDistance,
        };

        static void Update(
            TelemetryViewModel<MissionDistanceTelemetryData> t,
            MissionDistanceTelemetryData changes
        )
        {
            t.Text = changes.DistanceUnit.PrintFromSi(changes.TotalDistance, "F2");
            t.Units = changes.DistanceUnit.Symbol;
        }
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

#pragma warning disable SA1313
public record MissionDistanceTelemetryData(double TotalDistance, IUnitItem DistanceUnit);
#pragma warning restore SA1313
