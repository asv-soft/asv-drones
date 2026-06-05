using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class DistanceTelemetryItemViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public DistanceTelemetryItemViewModel()
        : this(
            nameof(DistanceTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(1000d).Concat(Observable.Never<double>()),
            MaterialIconKind.MapMarkerDistance,
            RS.MissionDistanceTelemetry_DisplayName,
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DistanceTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        Observable<double> distance,
        MaterialIconKind icon,
        string header,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, DistanceUnit.Id, distance, networkErrorTimeout)
    {
        ItemId = id;
        Icon = icon;
        Header = header;
        Status = defaultStatusColor;
        FormatString = "F2";
    }

    public string ItemId { get; }
}
