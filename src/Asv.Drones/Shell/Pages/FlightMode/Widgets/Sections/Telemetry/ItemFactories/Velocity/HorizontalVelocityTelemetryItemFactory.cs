using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class HorizontalVelocityTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "velocity";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var groundVelocity = device
            .GetRequiredMicroservice<IGnssClientEx>()
            .Main.GroundVelocity.Prepend(double.NaN);

        var velocityData = groundVelocity
            .CombineLatest(
                unitService.Units[VelocityUnit.Id].CurrentUnitItem,
                (ground, unit) => new VelocityTelemetryData(ground, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<VelocityTelemetryData>(Id, velocityData, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.HorizontalVelocityTelemetry_Header,
            ShortHeader = RS.HorizontalVelocityTelemetry_Header_Short,
            Icon = MaterialIconKind.Speedometer,
        };

        static void Update(
            TelemetryViewModel<VelocityTelemetryData> t,
            VelocityTelemetryData changes
        )
        {
            t.Text = changes.VelocityUnit.PrintFromSi(changes.GroundVelocity, "F2");
            t.Units = changes.VelocityUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record VelocityTelemetryData(double GroundVelocity, IUnitItem VelocityUnit);
#pragma warning restore SA1313
