// TODO: asv-soft-u08

using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public record AngleRttBoxData(
    double pitch,
    double roll,
    IUnitItem angleUnit
);

public class AngleUavRttIndicatorViewModel : TwoColumnRttBoxViewModel<AngleRttBoxData>
{
    [SetsRequiredMembers]
    public AngleUavRttIndicatorViewModel(
        NavigationId id, 
        ILoggerFactory loggerFactory, 
        ReactiveProperty<double> pitchAngle,
        ReactiveProperty<double> rollAngle,
        SynchronizedReactiveProperty<IUnitItem> currentAngleUnitItem,
        AsvColorKind defaultStatusColor) 
        : base(id, loggerFactory, pitchAngle
            .CombineLatest(
                rollAngle,
                currentAngleUnitItem,
                (agl, msl, unit) => new AngleRttBoxData(agl, msl, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200)),null)
    {
        Header = "Angle";
        Icon = MaterialIconKind.Altimeter;
        UpdateAction = (model, changes) =>
        {
            model.Left.ValueString = changes.angleUnit.PrintFromSi(
                changes.pitch,
                "F2"
            );
            model.Right.ValueString = changes.angleUnit.PrintFromSi(
                changes.roll,
                "F2"
            );

            model.Left.UnitSymbol = changes.angleUnit.Symbol;
            model.Right.UnitSymbol = changes.angleUnit.Symbol;
        };
        Status = defaultStatusColor;

        Left.Header = "Pitch";
        Right.Header = "Roll";
    }
}