using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class VelocityTelemetryItemViewModel : SplitDigitRttBoxViewModel, ITelemetryItem
{
    public VelocityTelemetryItemViewModel()
        : this(
            nameof(VelocityTelemetryItemViewModel),
            DesignTime.LoggerFactory,
            DeviceTelemetryDesignPreview.UnitService,
            Observable.Return(19.9d).Concat(Observable.Never<double>()),
            DeviceTelemetryDesignPreview.DefaultStatusColor
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public VelocityTelemetryItemViewModel(
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
