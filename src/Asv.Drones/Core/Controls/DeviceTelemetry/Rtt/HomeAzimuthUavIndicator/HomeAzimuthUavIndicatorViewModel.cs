using System;
using Asv.Avalonia;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class HomeAzimuthUavIndicatorViewModel : SplitDigitRttBoxViewModel
{
    public HomeAzimuthUavIndicatorViewModel()
        : this(
            new NavId(nameof(HomeAzimuthUavIndicator)),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            new ReactiveProperty<double>(30),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HomeAzimuthUavIndicatorViewModel(
        NavId id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        ReactiveProperty<double> homeAzimuth,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(
            id.TypeId,
            loggerFactory,
            unitService,
            AngleUnit.Id,
            homeAzimuth,
            networkErrorTimeout
        )
    {
        Header = RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth;
        ShortHeader = RS.HomeAzimuthUavIndicatorViewModel_HomeAzimuth_Short;
        Icon = MaterialIconKind.Home;
        Status = defaultStatusColor;
        FormatString = "F0";
    }
}
