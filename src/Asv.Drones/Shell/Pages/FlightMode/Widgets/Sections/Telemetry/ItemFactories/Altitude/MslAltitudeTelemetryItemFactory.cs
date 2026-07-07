using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class MslAltitudeTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "altitude-msl";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var altitudeObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Base.GlobalPosition.Select(pld => Math.Truncate((pld?.Alt ?? double.NaN) / 1000d))
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[AltitudeUnit.Id].CurrentUnitItem,
                (altitude, unit) => new MslAltitudeTelemetryData(altitude, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<MslAltitudeTelemetryData>(Id, altitudeObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.MslAltitudeTelemetry_Header,
            ShortHeader = RS.MslAltitudeTelemetry_Header_Short,
            Icon = MaterialIconKind.Altimeter,
        };

        static void Update(
            TelemetryViewModel<MslAltitudeTelemetryData> t,
            MslAltitudeTelemetryData changes
        )
        {
            t.Text = changes.AltitudeUnit.PrintFromSi(changes.Altitude, "F2");
            t.Units = changes.AltitudeUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record MslAltitudeTelemetryData(double Altitude, IUnitItem AltitudeUnit);
#pragma warning restore SA1313
