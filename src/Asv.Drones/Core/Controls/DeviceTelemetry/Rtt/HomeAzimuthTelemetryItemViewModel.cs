using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class HomeAzimuthTelemetryItemViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public HomeAzimuthTelemetryItemViewModel()
        : this(
            nameof(HomeAzimuthTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(30d).Concat(Observable.Never<double>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HomeAzimuthTelemetryItemViewModel(
        string id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        Observable<double> homeAzimuth,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, AngleUnit.Id, homeAzimuth, networkErrorTimeout)
    {
        ItemId = id;
        Header = RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth;
        ShortHeader = RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth_Short;
        Icon = MaterialIconKind.Home;
        Status = defaultStatusColor;
        FormatString = "F0";
    }

    public string ItemId { get; }
}
