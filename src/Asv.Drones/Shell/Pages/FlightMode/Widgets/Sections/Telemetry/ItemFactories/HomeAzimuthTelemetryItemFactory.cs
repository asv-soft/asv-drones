using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class HomeAzimuthTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "home-azimuth";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();
        var homeAzimuthObservable = positionClientEx
            .Current.Where(_ => positionClientEx.Home.CurrentValue.HasValue)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Select(p => p.Azimuth(positionClientEx.Home.CurrentValue ?? GeoPoint.NaN))
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[AngleUnit.Id].CurrentUnitItem,
                (azimuth, unit) => new HomeAzimuthRttBoxData(azimuth, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<HomeAzimuthRttBoxData>(Id, homeAzimuthObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.HomeAzimuthTelemetry_Header,
            ShortHeader = RS.HomeAzimuthTelemetry_Header_Short,
            Icon = MaterialIconKind.Home,
        };

        static void Update(
            TelemetryViewModel<HomeAzimuthRttBoxData> t,
            HomeAzimuthRttBoxData changes
        )
        {
            t.Text = changes.AngleUnit.PrintFromSi(changes.Azimuth, "F0");
            t.Units = changes.AngleUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record HomeAzimuthRttBoxData(double Azimuth, IUnitItem AngleUnit);
#pragma warning restore SA1313
