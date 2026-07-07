using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class MissionTargetTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "mission-target-distance";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null
        && device.GetMicroservice<IMissionClientEx>() is not null
        && device.GetMicroservice<IGnssClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();
        var missionClient = device.GetRequiredMicroservice<IMissionClientEx>();
        var gnssClient = device.GetRequiredMicroservice<IGnssClientEx>();
        var timeSpanUnit = unitService.Units[TimeSpanUnit.Id].AvailableUnits[
            TimeSpanHourMinuteSecondUnitItem.Id
        ];

        var targetData = positionClient
            .TargetDistance.Prepend(double.NaN)
            .CombineLatest(
                missionClient.Current.Prepend((ushort)0),
                gnssClient.Main.GroundVelocity.Prepend(double.NaN),
                unitService.Units[DistanceUnit.Id].CurrentUnitItem,
                (targetDistance, index, velocity, unit) =>
                    new TargetDistanceRttBoxData(
                        targetDistance,
                        index,
                        targetDistance / velocity,
                        unit,
                        timeSpanUnit
                    )
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<TargetDistanceRttBoxData>(Id, targetData, Update)
        {
            Density = TileDensity.Regular,
            Header = RS.MissionTargetTelemetry_Header,
            ShortHeader = RS.MissionTargetTelemetry_Header_Short,
            Icon = MaterialIconKind.Target,
        };

        static void Update(
            TelemetryViewModel<TargetDistanceRttBoxData> t,
            TargetDistanceRttBoxData changes
        )
        {
            t.Header = $"{RS.MissionTargetTelemetry_Header} {changes.TargetIndex}";
            t.Text = changes.DistanceUnit.PrintFromSi(changes.TargetDistance, "F2");
            t.Units = changes.DistanceUnit.Symbol;
            t.StatusText =
                double.IsNaN(changes.RemainingTime) || double.IsInfinity(changes.RemainingTime)
                    ? $"{RS.MissionTelemetry_RemainingTime}: -"
                    : $"{RS.MissionTelemetry_RemainingTime}: {changes.TimeSpanUnit.PrintFromSiWithUnits(changes.RemainingTime, "F0")}";
        }
    }
}

#pragma warning disable SA1313
public record TargetDistanceRttBoxData(
    double TargetDistance,
    ushort TargetIndex,
    double RemainingTime,
    IUnitItem DistanceUnit,
    IUnitItem TimeSpanUnit
);
#pragma warning restore SA1313
