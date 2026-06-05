using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class CurrentFlightModeTelemetryItemViewModel : SingleRttBoxViewModel<string>, ITelemetryItem
{
    [SetsRequiredMembers]
    public CurrentFlightModeTelemetryItemViewModel()
        : this(
            nameof(CurrentFlightModeTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            Observable.Return("Guided").Concat(Observable.Never<string>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [SetsRequiredMembers]
    public CurrentFlightModeTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        Observable<string> currentMode,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, currentMode.ObserveOnUIThreadDispatcher(), networkErrorTimeout)
    {
        ItemId = id;
        Header = RS.UavRttItem_Mode;
        Icon = MaterialIconKind.FlightMode;
        UpdateAction = (model, mode) => model.ValueString = mode;
        Status = defaultStatusColor;
    }

    public string ItemId { get; }
}
