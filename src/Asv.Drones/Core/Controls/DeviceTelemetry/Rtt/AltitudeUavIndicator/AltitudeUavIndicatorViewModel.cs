using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record AltitudeRttBoxData(double AltitudeAgl, double AltitudeMsl, IUnitItem AltitudeUnit);
#pragma warning restore SA1313

public class AltitudeUavIndicatorViewModel : TwoColumnRttBoxViewModel<AltitudeRttBoxData>
{
    [SetsRequiredMembers]
    public AltitudeUavIndicatorViewModel()
        : this(
            nameof(AltitudeUavIndicator),
            DesignTime.LoggerFactory,
            new ReactiveProperty<double>(10),
            new ReactiveProperty<double>(14),
            DeviceTelemetryDesignPreview.Unit(AltitudeUnit.Id).CurrentUnitItem,
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public AltitudeUavIndicatorViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        ReactiveProperty<double> altitudeAgl,
        ReactiveProperty<double> altitudeMsl,
        SynchronizedReactiveProperty<IUnitItem> currentUnitItem,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            altitudeAgl
                .CombineLatest(
                    altitudeMsl,
                    currentUnitItem,
                    (agl, msl, unit) => new AltitudeRttBoxData(agl, msl, unit)
                )
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        Header = RS.UavRttItem_Altitude;
        Icon = MaterialIconKind.Altimeter;
        UpdateAction = (model, changes) =>
        {
            model.Left.ValueString = changes.AltitudeUnit.PrintFromSi(changes.AltitudeAgl, "F2");
            model.Right.ValueString = changes.AltitudeUnit.PrintFromSi(changes.AltitudeMsl, "F2");

            model.Left.UnitSymbol = changes.AltitudeUnit.Symbol;
            model.Right.UnitSymbol = changes.AltitudeUnit.Symbol;
        };
        Status = defaultStatusColor;

        Left.Header = RS.AltitudeUavIndicatorViewModel_Agl;
        Right.Header = RS.AltitudeUavIndicatorViewModel_Msl;
    }
}
