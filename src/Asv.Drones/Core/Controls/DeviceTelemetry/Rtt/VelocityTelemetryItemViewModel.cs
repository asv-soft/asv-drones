using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record VelocityRttBoxData(
    double GroundVelocity,
    double VerticalVelocity,
    IUnitItem VelocityUnit
);
#pragma warning restore SA1313

public class VelocityTelemetryItemViewModel
    : TwoColumnRttBoxViewModel<VelocityRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public VelocityTelemetryItemViewModel()
        : this(
            nameof(VelocityTelemetryItemViewModel),
            DeviceTelemetryDesignPreview
                .UnitService.Units[VelocityUnit.Id]
                .CurrentUnitItem.Select(unit => new VelocityRttBoxData(19.9d, 2.5d, unit)),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public VelocityTelemetryItemViewModel(
        string id,
        Observable<VelocityRttBoxData> velocityData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            velocityData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        Header = RS.UavRttItem_Velocity;
        Icon = MaterialIconKind.Speedometer;
        UpdateAction = (model, changes) =>
        {
            model.Left.ValueString = changes.VelocityUnit.PrintFromSi(changes.GroundVelocity, "F2");
            model.Right.ValueString = changes.VelocityUnit.PrintFromSi(
                changes.VerticalVelocity,
                "F2"
            );

            model.Left.UnitSymbol = changes.VelocityUnit.Symbol;
            model.Right.UnitSymbol = changes.VelocityUnit.Symbol;
        };
        Status = defaultStatusColor;

        Left.Header = RS.VelocityUavIndicatorViewModel_Velocity_Short;
        Right.Header = RS.VelocityUavIndicatorViewModel_Vertical_Short;
    }

    public string ItemId { get; }
}
