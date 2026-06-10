using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class HeadingTelemetryItemViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public HeadingTelemetryItemViewModel()
        : this(
            nameof(HeadingTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(29d).Concat(Observable.Never<double>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HeadingTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        Observable<double> heading,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, AngleUnit.Id, heading, networkErrorTimeout)
    {
        ItemId = id;
        Header = RS.HeadingUavIndicatorViewModel_Heading;
        ShortHeader = RS.HeadingUavIndicatorViewModel_Heading_Short;
        Icon = MaterialIconKind.SunAzimuth;
        Status = defaultStatusColor;
        FormatString = "F0";
    }

    public string ItemId { get; }
}
