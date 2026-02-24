// TODO: asv-soft-u08

using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public record AltitudeRttBoxData(
    double altitudeAgl,
    double altitudeMsl,
    IUnitItem altitudeUnit
);

public class AltitudeUavIndicatorViewModel : TwoColumnRttBoxViewModel<AltitudeRttBoxData>
{
    [SetsRequiredMembers]
    public AltitudeUavIndicatorViewModel(
        NavigationId id, 
        ILoggerFactory loggerFactory, 
        ReactiveProperty<double> altitudeAgl,
        ReactiveProperty<double> altitudeMsl,
        SynchronizedReactiveProperty<IUnitItem> currentUnitItem,
        AsvColorKind defaultStatusColor) 
        : base(id, loggerFactory, altitudeAgl
            .CombineLatest(
                altitudeMsl,
                currentUnitItem,
                (agl, msl, unit) => new AltitudeRttBoxData(agl, msl, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200)),null)
    {
        Header = RS.UavRttItem_Altitude;
        Icon = MaterialIconKind.Altimeter;
        UpdateAction = (model, changes) =>
        {
            model.Left.ValueString = changes.altitudeUnit.PrintFromSi(
                changes.altitudeAgl,
                "F2"
            );
            model.Right.ValueString = changes.altitudeUnit.PrintFromSi(
                changes.altitudeMsl,
                "F2"
            );

            model.Left.UnitSymbol = changes.altitudeUnit.Symbol;
            model.Right.UnitSymbol = changes.altitudeUnit.Symbol;
        };
        Status = defaultStatusColor;
        
        Left.Header = "AGL";
        Right.Header = "MSL";
    }
}
