using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class VelocityUavIndicatorViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public VelocityUavIndicatorViewModel()
        : this(
            nameof(VelocityUavIndicator),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(19.9d),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public VelocityUavIndicatorViewModel(
        string id,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        Observable<double> velocity,
        AsvColorKind defaultStatusColor,
        TimeSpan? networkErrorTimeout = null
    )
        : base(id, loggerFactory, unitService, VelocityUnit.Id, velocity, networkErrorTimeout)
    {
        ItemId = id;
        Header = RS.UavRttItem_Velocity;
        ShortHeader = RS.VelocityUavIndicatorViewModel_Velocity_Short;
        Icon = MaterialIconKind.Speedometer;
        Status = defaultStatusColor;
        FormatString = "F2";
    }

    public string ItemId { get; }
}
