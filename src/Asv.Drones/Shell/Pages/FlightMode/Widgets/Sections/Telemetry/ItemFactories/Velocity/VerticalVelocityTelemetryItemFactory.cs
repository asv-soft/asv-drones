using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class VerticalVelocityTelemetryItemFactory(IUnitService unitService)
    : ITelemetryItemFactory
{
    public const string Id = "vertical-velocity";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var verticalVelocity = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Base.VfrHud.Select(pld => (double?)pld?.Climb ?? double.NaN)
            .Prepend(double.NaN);

        var velocityData = verticalVelocity
            .CombineLatest(
                unitService.Units[VelocityUnit.Id].CurrentUnitItem,
                (vertical, unit) => new VerticalVelocityTelemetryData(vertical, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<VerticalVelocityTelemetryData>(Id, velocityData, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.VerticalVelocityTelemetry_Header,
            ShortHeader = RS.VerticalVelocityTelemetry_Header_Short,
            Icon = MaterialIconKind.Speedometer,
        };

        static void Update(
            TelemetryViewModel<VerticalVelocityTelemetryData> t,
            VerticalVelocityTelemetryData changes
        )
        {
            t.Text = changes.VelocityUnit.PrintFromSi(changes.VerticalVelocity, "F2");
            t.Units = changes.VelocityUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record VerticalVelocityTelemetryData(double VerticalVelocity, IUnitItem VelocityUnit);
#pragma warning restore SA1313
