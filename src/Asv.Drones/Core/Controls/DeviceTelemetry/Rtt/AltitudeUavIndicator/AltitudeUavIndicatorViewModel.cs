using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record AltitudeRttBoxData(double AltitudeAgl, double AltitudeMsl, IUnitItem AltitudeUnit);
#pragma warning restore SA1313

public class AltitudeUavIndicatorViewModel
    : TwoColumnRttBoxViewModel<AltitudeRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public AltitudeUavIndicatorViewModel()
        : this(
            nameof(AltitudeUavIndicator),
            DeviceTelemetryDesignPreview
                .UnitService.Units[AltitudeUnit.Id]
                .CurrentUnitItem.Select(unit => new AltitudeRttBoxData(10d, 14d, unit)),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public AltitudeUavIndicatorViewModel(
        string id,
        Observable<AltitudeRttBoxData> altitudeData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            altitudeData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
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

    public string ItemId { get; }
}
