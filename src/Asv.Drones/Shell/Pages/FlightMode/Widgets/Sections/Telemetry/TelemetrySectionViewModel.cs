using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class TelemetrySectionViewModel : DashboardViewModel, ITelemetrySection
{
    public const string SectionId = "telemetry-widget-section";

    public TelemetrySectionViewModel()
        : base(DesignTime.Id.ToString())
    {
        DesignTime.ThrowIfNotDesignMode();

        Tiles.AddRange(CreateDesignTimeTiles());
    }

    public TelemetrySectionViewModel(
        TelemetrySectionArgs args,
        ILoggerFactory loggerFactory,
        IEnumerable<ITelemetryItemFactory> factories
    )
        : base(SectionId)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(factories);
        var device = args.Device;
        var facs = factories as ITelemetryItemFactory[] ?? factories.ToArray();

        foreach (var factory in facs.Where(f => f.CanCreate(device)))
        {
            Tiles.Add(factory.Create(device));
        }
    }

    public int Order => 2;

    private static IEnumerable<ITileViewModel> CreateDesignTimeTiles()
    {
        var altitudeUnit = DeviceTelemetryDesignPreview.Unit(AltitudeUnit.Id).AvailableUnits[
            AltitudeMeterUnitItem.Id
        ];
        var progressUnit = DeviceTelemetryDesignPreview.Unit(ProgressUnit.Id).AvailableUnits[
            ProgressPercentUnitItem.Id
        ];

        yield return new TelemetryViewModel<double>(
            AltitudeTelemetryItemFactory.Id,
            Observable.Return(128.4d),
            (tile, altitude) =>
            {
                tile.Text = altitudeUnit.PrintFromSi(altitude, "F1");
                tile.Units = altitudeUnit.Symbol;
                tile.StatusIcon = MaterialIconKind.CheckCircle;
                tile.StatusIconColor = AsvColorKind.Success;
            }
        )
        {
            Density = TileDensity.Regular,
            Header = RS.AltitudeTelemetry_Header,
            ShortHeader = RS.AltitudeTelemetry_Header_Short,
            Icon = MaterialIconKind.Altimeter,
        };

        yield return new TelemetryViewModel<double>(
            BatteryTelemetryItemFactory.Id,
            Observable.Return(0.76d),
            (tile, charge) =>
            {
                var percentCharge = charge * 100d;
                tile.Text = progressUnit.PrintFromSi(percentCharge, "F0");
                tile.Units = progressUnit.Symbol;
                tile.Progress = percentCharge;
                tile.ProgressColor = AsvColorKind.Success;
                tile.StatusColor = AsvColorKind.Success;
                tile.StatusIcon = MaterialIconKind.CheckCircle;
                tile.StatusIconColor = AsvColorKind.Success;
            }
        )
        {
            Density = TileDensity.Regular,
            Header = RS.BatteryTelemetry_Header,
            ShortHeader = RS.BatteryTelemetry_Header_Short,
            Icon = MaterialIconKind.Battery80,
        };

        yield return new TelemetryViewModel<int>(
            GnssSatellitesTelemetryItemFactory.Id,
            Observable.Return(18),
            (tile, satellites) =>
            {
                tile.Text = satellites.ToString();
                tile.Units = RS.GnssTelemetry_Satellites_Unit;
                tile.StatusIcon = MaterialIconKind.CheckCircle;
                tile.StatusIconColor = AsvColorKind.Success;
            }
        )
        {
            Density = TileDensity.Inline,
            Header = RS.GnssTelemetry_Satellites_Header,
            ShortHeader = RS.GnssTelemetry_Satellites_Header_Short,
            Icon = MaterialIconKind.GpsFixed,
        };

        yield return new TelemetryViewModel<string>(
            CurrentFlightModeTelemetryItemFactory.Id,
            Observable.Return("Auto"),
            static (tile, mode) => tile.Text = mode
        )
        {
            Density = TileDensity.Inline,
            Header = RS.ModeTelemetry_Header,
            ShortHeader = RS.ModeTelemetry_Header,
            Icon = MaterialIconKind.FlightMode,
        };
    }
}
