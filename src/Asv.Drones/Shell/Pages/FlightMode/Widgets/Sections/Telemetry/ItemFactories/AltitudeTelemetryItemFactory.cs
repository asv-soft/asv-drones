using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record AltitudeRttBoxData(double AltitudeAgl, double AltitudeMsl, IUnitItem AltitudeUnit);
#pragma warning restore SA1313

public sealed class AltitudeTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "altitude";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();

        var altitudeObservable = positionClientEx
            .Base.GlobalPosition.Select(pld =>
                (
                    AltitudeAgl: Math.Truncate((pld?.RelativeAlt ?? double.NaN) / 1000d),
                    AltitudeMsl: Math.Truncate((pld?.Alt ?? double.NaN) / 1000d)
                )
            )
            .Prepend((AltitudeAgl: double.NaN, AltitudeMsl: double.NaN))
            .CombineLatest(
                unitService.Units[AltitudeUnit.Id].CurrentUnitItem,
                (value, unit) => new AltitudeRttBoxData(value.AltitudeAgl, value.AltitudeMsl, unit)
            );

        return InternalCreate(altitudeObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var altitudeObservable = unitService
            .Units[AltitudeUnit.Id]
            .CurrentUnitItem.Select(unit => new AltitudeRttBoxData(10d, 14d, unit));

        return InternalCreate(altitudeObservable);
    }

    private IRttBoxViewModel InternalCreate(Observable<AltitudeRttBoxData> observable)
    {
        var rtt = new TwoColumnRttBoxViewModel<AltitudeRttBoxData>(Id, observable, null)
        {
            Header = RS.UavRttItem_Altitude,
            Icon = MaterialIconKind.Altimeter,
            UpdateAction = (model, changes) =>
            {
                model.Left.ValueString = changes.AltitudeUnit.PrintFromSi(
                    changes.AltitudeAgl,
                    "F2"
                );
                model.Right.ValueString = changes.AltitudeUnit.PrintFromSi(
                    changes.AltitudeMsl,
                    "F2"
                );

                model.Left.UnitSymbol = changes.AltitudeUnit.Symbol;
                model.Right.UnitSymbol = changes.AltitudeUnit.Symbol;
            },
            Status = DefaultStatusColor,
        };

        rtt.Left.Header = RS.AltitudeUavIndicatorViewModel_Agl;
        rtt.Right.Header = RS.AltitudeUavIndicatorViewModel_Msl;

        return rtt;
    }
}
