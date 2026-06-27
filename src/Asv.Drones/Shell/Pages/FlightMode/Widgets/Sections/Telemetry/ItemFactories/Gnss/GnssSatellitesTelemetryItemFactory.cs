using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class GnssSatellitesTelemetryItemFactory : ITelemetryItemFactory
{
    public const string Id = "gnss-satellites";

    private const int DangerSatelliteCount = 10;
    private static readonly Range WarningSatelliteAmount = 15..20;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var gnssClient = device.GetRequiredMicroservice<IGnssClientEx>();
        var gnssObservable = gnssClient
            .Main.Info.Select(info => new GnssRttBoxData(info.SatellitesVisible, info.FixType))
            .Prepend(new GnssRttBoxData(0, Mavlink.Common.GpsFixType.GpsFixTypeNoGps))
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<GnssRttBoxData>(Id, gnssObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.GnssTelemetry_Satellites_Header,
            ShortHeader = RS.GnssTelemetry_Satellites_Header_Short,
            Icon = MaterialIconKind.GpsFixed,
        };

        static void Update(TelemetryViewModel<GnssRttBoxData> t, GnssRttBoxData changes)
        {
            t.Text = changes.Satellites.ToString();
            t.Units = RS.GnssTelemetry_Satellites_Unit;
            t.StatusIcon = GetGnssStatusIcon(changes.Satellites, changes.Mode);
            t.StatusIconColor = GetGnssStatusIconColor(changes.Satellites, changes.Mode);
        }
    }

    private static AsvColorKind GetGnssStatusIconColor(
        int satellitesCount,
        Mavlink.Common.GpsFixType mode
    )
    {
        if (
            mode == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || satellitesCount > WarningSatelliteAmount.Start.Value
            || satellitesCount < WarningSatelliteAmount.End.Value
        )
        {
            return AsvColorKind.Warning;
        }

        if (
            mode != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed
            || satellitesCount < DangerSatelliteCount
        )
        {
            return AsvColorKind.Error;
        }

        return TelemetryHelper.DefaultStatusColor;
    }

    private static MaterialIconKind GetGnssStatusIcon(
        int satellitesCount,
        Mavlink.Common.GpsFixType mode
    )
    {
        if (
            mode == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || satellitesCount > WarningSatelliteAmount.Start.Value
            || satellitesCount < WarningSatelliteAmount.End.Value
            || mode != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed
            || satellitesCount < DangerSatelliteCount
        )
        {
            return MaterialIconKind.WarningCircle;
        }

        return MaterialIconKind.CheckCircle;
    }
}

#pragma warning disable SA1313
public record GnssRttBoxData(int Satellites, Mavlink.Common.GpsFixType Mode);
#pragma warning restore SA1313
