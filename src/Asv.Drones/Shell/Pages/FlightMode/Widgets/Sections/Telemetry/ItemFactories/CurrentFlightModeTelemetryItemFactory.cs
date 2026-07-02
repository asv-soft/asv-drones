using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class CurrentFlightModeTelemetryItemFactory : ITelemetryItemFactory
{
    public const string Id = "current-flight-mode";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IModeClient>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var currentMode = device
            .GetRequiredMicroservice<IModeClient>()
            .CurrentMode.Select(mode => mode.Name)
            .Prepend(string.Empty)
            .Select(mode => new FlightModeTelemetryData(mode))
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<FlightModeTelemetryData>(Id, currentMode, Update)
        {
            Density = TileDensity.Regular,
            Header = RS.ModeTelemetry_Header,
            ShortHeader = RS.ModeTelemetry_Header,
            Icon = MaterialIconKind.FlightMode,
        };

        static void Update(
            TelemetryViewModel<FlightModeTelemetryData> t,
            FlightModeTelemetryData changes
        )
        {
            t.Text = changes.Mode;
        }
    }
}

#pragma warning disable SA1313
public record FlightModeTelemetryData(string Mode);
#pragma warning restore SA1313
