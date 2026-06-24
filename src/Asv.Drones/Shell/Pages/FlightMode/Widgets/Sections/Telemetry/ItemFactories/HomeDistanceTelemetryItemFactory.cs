using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class HomeDistanceTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "home-distance";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClient = device.GetRequiredMicroservice<IPositionClientEx>();
        var distance = positionClient
            .HomeDistance.Skip(1)
            .DistinctUntilChanged()
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[DistanceUnit.Id].CurrentUnitItem,
                (distance, unit) => new HomeDistanceRttBoxData(distance, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<HomeDistanceRttBoxData>(Id, distance, Update)
        {
            Density = TileDensity.Inline,
            Icon = MaterialIconKind.Home,
            Header = RS.HomeDistanceTelemetry_Header,
            ShortHeader = RS.HomeDistanceTelemetry_Header,
        };

        static void Update(
            TelemetryViewModel<HomeDistanceRttBoxData> t,
            HomeDistanceRttBoxData changes
        )
        {
            t.Text = changes.DistanceUnit.PrintFromSi(changes.Distance, "F2");
            t.Units = changes.DistanceUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record HomeDistanceRttBoxData(double Distance, IUnitItem DistanceUnit);
#pragma warning restore SA1313
