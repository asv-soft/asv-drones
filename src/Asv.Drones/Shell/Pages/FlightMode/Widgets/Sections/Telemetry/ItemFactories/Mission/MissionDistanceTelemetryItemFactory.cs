using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class MissionDistanceTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "mission-path-distance";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IMissionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var missionDistance = device
            .GetRequiredMicroservice<IMissionClientEx>()
            .AllMissionsDistance.Select(distance => distance * 1000)
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[DistanceUnit.Id].CurrentUnitItem,
                (distance, unit) => new MissionPathDistanceTelemetryData(distance, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<MissionPathDistanceTelemetryData>(Id, missionDistance, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.MissionDistanceTelemetry_Header,
            ShortHeader = RS.MissionDistanceTelemetry_Header_Short,
            Icon = MaterialIconKind.LocationDistance,
        };

        static void Update(
            TelemetryViewModel<MissionPathDistanceTelemetryData> t,
            MissionPathDistanceTelemetryData changes
        )
        {
            t.Text = changes.DistanceUnit.PrintFromSi(changes.Distance, "F2");
            t.Units = changes.DistanceUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record MissionPathDistanceTelemetryData(double Distance, IUnitItem DistanceUnit);
#pragma warning restore SA1313
