using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class TimeInAirTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "time-in-air";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var timeSpanUnit = unitService.Units[TimeSpanUnit.Id].AvailableUnits[
            TimeSpanHourMinuteSecondUnitItem.Id
        ];

        var timeInAir = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .ArmedTime.Select(time => time.TotalSeconds)
            .Prepend(double.NaN)
            .Select(time => new TimeInAirTelemetryData(time, timeSpanUnit))
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<TimeInAirTelemetryData>(Id, timeInAir, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.TimeInAirTelemetry_Header,
            ShortHeader = RS.TimeInAirTelemetry_Header_Short,
            Icon = MaterialIconKind.ClockOutline,
        };

        static void Update(
            TelemetryViewModel<TimeInAirTelemetryData> t,
            TimeInAirTelemetryData changes
        )
        {
            t.Text = double.IsNaN(changes.TimeInAir)
                ? "-"
                : changes.TimeSpanUnit.PrintFromSi(changes.TimeInAir, "F0");
            t.Units = changes.TimeSpanUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record TimeInAirTelemetryData(double TimeInAir, IUnitItem TimeSpanUnit);
#pragma warning restore SA1313
