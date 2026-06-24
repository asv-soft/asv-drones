using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class AltitudeTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "altitude";
    private const int CriticalAltitude = 40;
    private const int CriticalVelocity = 10;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();
        var gnssClientEx = device.GetMicroservice<IGnssClientEx>();

        var observable = positionClientEx
            .Base.GlobalPosition.Select(pld =>
                Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d)
            )
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[AltitudeUnit.Id].CurrentUnitItem,
                (altitude, unit) =>
                    new AltitudeTelemetryData(
                        altitude,
                        Math.Round(gnssClientEx?.Main.GroundVelocity.CurrentValue ?? double.NaN),
                        unit
                    )
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<AltitudeTelemetryData>(Id, observable, Update)
        {
            Density = TileDensity.Regular,
            Header = RS.AltitudeTelemetry_Header,
            ShortHeader = RS.AltitudeTelemetry_Header_Short,
            Icon = MaterialIconKind.Altimeter,
        };

        static void Update(
            TelemetryViewModel<AltitudeTelemetryData> t,
            AltitudeTelemetryData changes
        )
        {
            t.Text = changes.AltitudeUnit.PrintFromSi(changes.Altitude, "F2");
            t.Units = changes.AltitudeUnit.Symbol;
            var isPullUpRequired = IsPullUpRequired(changes);
            t.StatusText = isPullUpRequired ? RS.AltitudeTelemetry_StatusText_PullUp : null;
            t.StatusTextColor = isPullUpRequired
                ? AsvColorKind.Error | AsvColorKind.Blink
                : TelemetryHelper.DefaultStatusColor;
            t.StatusIcon = isPullUpRequired ? MaterialIconKind.AlertCircle : null;
            t.StatusIconColor = isPullUpRequired
                ? AsvColorKind.Error | AsvColorKind.Blink
                : AsvColorKind.None;
        }
    }

    private static bool IsPullUpRequired(AltitudeTelemetryData data) =>
        data is { GroundVelocity: > CriticalVelocity, Altitude: < CriticalAltitude };
}

#pragma warning disable SA1313
public record AltitudeTelemetryData(double Altitude, double GroundVelocity, IUnitItem AltitudeUnit);
#pragma warning restore SA1313
