using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class VelocityTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "velocity";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null
        && device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
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

        return InternalCreate(velocityData);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var velocityData = unitService
            .Units[VelocityUnit.Id]
            .CurrentUnitItem.Select(unit => new VelocityRttBoxData(19.9d, 2.5d, unit));

        return InternalCreate(velocityData);
    }

    private static IRttBoxViewModel InternalCreate(Observable<VelocityRttBoxData> observable)
    {
        var rtt = new TwoColumnRttBoxViewModel<VelocityRttBoxData>(Id, observable, null)
        {
            Header = RS.UavRttItem_Velocity,
            Icon = MaterialIconKind.Speedometer,
            UpdateAction = (model, changes) =>
            {
                model.Left.ValueString = changes.VelocityUnit.PrintFromSi(
                    changes.GroundVelocity,
                    "F2"
                );
                model.Right.ValueString = changes.VelocityUnit.PrintFromSi(
                    changes.VerticalVelocity,
                    "F2"
                );

                model.Left.UnitSymbol = changes.VelocityUnit.Symbol;
                model.Right.UnitSymbol = changes.VelocityUnit.Symbol;
            },
            Status = DefaultStatusColor,
        };

        rtt.Left.Header = RS.VelocityUavIndicatorViewModel_Velocity_Short;
        rtt.Right.Header = RS.VelocityUavIndicatorViewModel_Vertical_Short;

        return rtt;
    }
}

#pragma warning disable SA1313
public record VelocityRttBoxData(
    double GroundVelocity,
    double VerticalVelocity,
    IUnitItem VelocityUnit
);
#pragma warning restore SA1313
