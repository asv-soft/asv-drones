using System;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class HeadingUavIndicatorViewModel : SplitDigitRttBoxViewModel
{
    public HeadingUavIndicatorViewModel()
        : this(
            nameof(HeadingUavIndicator),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            new ReactiveProperty<double>(29),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HeadingUavIndicatorViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        ReactiveProperty<double> heading,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, AngleUnit.Id, heading, networkErrorTimeout)
    {
        Header = RS.HeadingUavIndicatorViewModel_Heading;
        ShortHeader = RS.HeadingUavIndicatorViewModel_Heading_Short;
        Icon = MaterialIconKind.SunAzimuth;
        Status = defaultStatusColor;
        FormatString = "F0";
    }
}
