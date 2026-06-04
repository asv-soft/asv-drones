using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class HomeAzimuthUavIndicatorViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public HomeAzimuthUavIndicatorViewModel()
        : this(
            nameof(HomeAzimuthUavIndicator),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(30d),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HomeAzimuthUavIndicatorViewModel(
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
