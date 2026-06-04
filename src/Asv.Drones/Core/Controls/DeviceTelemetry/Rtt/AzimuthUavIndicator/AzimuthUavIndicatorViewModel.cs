using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class AzimuthUavIndicatorViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public AzimuthUavIndicatorViewModel()
        : this(
            nameof(AzimuthUavIndicator),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(39d).Concat(Observable.Never<double>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public AzimuthUavIndicatorViewModel(
        string id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        Observable<double> azimuth,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, AngleUnit.Id, azimuth, networkErrorTimeout)
    {
        ItemId = id;
        Header = RS.UavRttItem_Azimuth;
        Icon = MaterialIconKind.SunAzimuth;
        Status = defaultStatusColor;
        FormatString = "F2";
    }

    public string ItemId { get; }
}
