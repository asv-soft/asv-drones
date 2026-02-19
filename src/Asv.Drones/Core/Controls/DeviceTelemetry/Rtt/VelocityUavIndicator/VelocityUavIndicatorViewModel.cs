using System;
using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class VelocityUavIndicatorViewModel : SplitDigitRttBoxViewModel
{
    public VelocityUavIndicatorViewModel()
        : this(
            nameof(VelocityUavIndicator),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            new ReactiveProperty<double>(19.9),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public VelocityUavIndicatorViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        ReactiveProperty<double> velocity,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, VelocityUnit.Id, velocity, networkErrorTimeout)
    {
        Header = RS.UavRttItem_Velocity;
        ShortHeader = RS.VelocityUavIndicatorViewModel_Velocity_Short;
        Icon = MaterialIconKind.Speedometer;
        Status = defaultStatusColor;
        FormatString = "F2";
    }
}
