using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public sealed class VelocityTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "velocity";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;
    public string DisplayName => RS.UavRttItem_Velocity;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null
        && device.GetMicroservice<IPositionClientEx>() is not null;

    public ITelemetryItem Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var groundVelocity = device
            .GetRequiredMicroservice<IGnssClientEx>()
            .Main.GroundVelocity.Prepend(double.NaN);

        var verticalVelocity = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Base.VfrHud.Select(pld => (double?)pld?.Climb ?? double.NaN)
            .Prepend(double.NaN);

        var velocityData = groundVelocity.CombineLatest(
            verticalVelocity,
            unitService.Units[VelocityUnit.Id].CurrentUnitItem,
            (ground, vertical, unit) => new VelocityRttBoxData(ground, vertical, unit)
        );

        return new VelocityTelemetryItemViewModel(Id, velocityData, DefaultStatusColor);
    }

    public ITelemetryItem CreatePreview()
    {
        var velocityData = unitService
            .Units[VelocityUnit.Id]
            .CurrentUnitItem.Select(unit => new VelocityRttBoxData(19.9d, 2.5d, unit));

        return new VelocityTelemetryItemViewModel(Id, velocityData, DefaultStatusColor);
    }
}
