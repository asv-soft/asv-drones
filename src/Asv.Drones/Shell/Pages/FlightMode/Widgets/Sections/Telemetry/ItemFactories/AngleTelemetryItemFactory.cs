using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class AngleTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "angle";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public IRttBoxViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();

        var angleObservable = positionClientEx
            .Pitch.Prepend(double.NaN)
            .CombineLatest(
                positionClientEx.Roll.Prepend(double.NaN),
                unitService.Units[AngleUnit.Id].CurrentUnitItem,
                (pitch, roll, unit) => new AngleRttBoxData(pitch, roll, unit)
            );

        return InternalCreate(angleObservable);
    }

    public IRttBoxViewModel CreatePreview()
    {
        var angleObservable = unitService
            .Units[AngleUnit.Id]
            .CurrentUnitItem.Select(unit => new AngleRttBoxData(30d, 10d, unit));

        return InternalCreate(angleObservable);
    }

    private static IRttBoxViewModel InternalCreate(Observable<AngleRttBoxData> observable)
    {
        var rtt = new TwoColumnRttBoxViewModel<AngleRttBoxData>(Id, observable, null)
        {
            Header = RS.AngleUavRttIndicatorViewModel_Angle,
            Icon = MaterialIconKind.Altimeter,
            UpdateAction = (model, changes) =>
            {
                model.Left.ValueString = changes.AngleUnit.PrintFromSi(changes.Pitch, "F2");
                model.Right.ValueString = changes.AngleUnit.PrintFromSi(changes.Roll, "F2");

                model.Left.UnitSymbol = changes.AngleUnit.Symbol;
                model.Right.UnitSymbol = changes.AngleUnit.Symbol;
            },
            Status = DefaultStatusColor,
        };

        rtt.Left.Header = RS.AngleUavRttIndicatorViewModel_Pitch;
        rtt.Right.Header = RS.AngleUavRttIndicatorViewModel_Roll;

        return rtt;
    }
}

#pragma warning disable SA1313
public record AngleRttBoxData(double Pitch, double Roll, IUnitItem AngleUnit);
#pragma warning restore SA1313
