using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

#pragma warning disable SA1313
public record FlightModeRttBoxData(string Mode, double TimeInAir, IUnitItem TimeSpanUnit);
#pragma warning restore SA1313

public class CurrentFlightModeTelemetryItemViewModel
    : KeyValueRttBoxViewModel<FlightModeRttBoxData>,
        ITelemetryItem
{
    [SetsRequiredMembers]
    public CurrentFlightModeTelemetryItemViewModel()
        : this(
            nameof(CurrentFlightModeTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            Observable
                .Return(
                    new FlightModeRttBoxData("Guided", 125d, new TimeSpanHourMinuteSecondUnitItem())
                )
                .Concat(Observable.Never<FlightModeRttBoxData>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public CurrentFlightModeTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        Observable<FlightModeRttBoxData> modeData,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id,
            loggerFactory,
            modeData.ObserveOnUIThreadDispatcher().ThrottleLast(TimeSpan.FromMilliseconds(200)),
            networkErrorTimeout
        )
    {
        ItemId = id;
        Header = RS.UavRttItem_Mode;
        Icon = MaterialIconKind.FlightMode;
        UpdateAction = (model, changes) =>
        {
            model[0, RS.UavRttItem_Mode, null].ValueString = changes.Mode;
            model[1, RS.UavRttItem_TimeInAir, changes.TimeSpanUnit.Symbol].ValueString =
                double.IsNaN(changes.TimeInAir)
                    ? "-"
                    : changes.TimeSpanUnit.PrintFromSi(changes.TimeInAir, "F0");
        };
        Status = defaultStatusColor;
    }

    public string ItemId { get; }
}
